#include <iostream>
#include <vector>
#include <map>
#include <fstream>
#include <string>
#include <iterator>
#include <functional>
#include <algorithm>
#include <sstream>

std::string trim_begin(const std::string& str) {
    auto alpha = std::find_if_not(str.begin(), str.end(), ::isspace);
    return str.substr(std::distance(str.begin(), alpha));
}

std::string trim_end(const std::string& str) {
    auto alpha = std::find_if_not(str.rbegin(), str.rend(), ::isspace).base();
    return str.substr(0, std::distance(str.begin(), alpha));
}

std::vector<std::string> extract_words(const std::string& str) {
    std::vector<std::string> words;
    std::istringstream is(str);
    std::copy(
        std::istream_iterator<std::string>(is),
        std::istream_iterator<std::string>(),
        std::back_inserter(words));
    return words;
}

class sentence_file_reader {
    std::ifstream ifs;
    static const size_t BUFFER_SIZE = 16 * 1024;
    char buff[BUFFER_SIZE];

public:
    sentence_file_reader(const char *filename) :
        ifs(filename, std::ios::in)
    {}

    ~sentence_file_reader() {
        ifs.close();
    }

    std::string get_next_sentence() {
        ifs.getline(buff, BUFFER_SIZE, '.');
        return trim_begin(trim_end(buff));
    }
    bool has_more() {
        return ifs.good();
    }
};

struct word_node {
    word_node(const std::string& name) :
    word_name(name)
    {}

    std::string word_name;
    std::map<word_node*, size_t> lefts;
    std::map<word_node*, size_t> rights;
};

struct word_graph {
    std::map<std::string, word_node*> word_nodes;
    word_node *head;

    word_node *get_or_create(std::string name) {
        auto name_exists = word_nodes.equal_range(name);
        if (name_exists.first == name_exists.second) {
            word_node *name_node = new word_node(name);
            return word_nodes.insert(name_exists.second, std::make_pair(name, name_node))->second;
        }
        return name_exists.first->second;
    }

    void make_link(const std::string& a, const std::string& b) {
        word_node *a_node = get_or_create(a);
        word_node *b_node = get_or_create(b);
        a_node->rights[b_node]++;
        b_node->lefts[a_node]++;
    }
};

int main() {
    std::ios::ios_base::sync_with_stdio(false);

    std::cout << "Hello World!\n";

    word_graph graph;

    sentence_file_reader cfr("test.txt");
    std::string str;
    while (cfr.has_more()) {
        std::string sentence = cfr.get_next_sentence();
        std::vector<std::string> words = extract_words(sentence);

        if (words.size() > 1) {
            graph.make_link(words[0], words[1]);
            for (size_t i = 1, j = 2; j < words.size(); ++i, ++j) {
                graph.make_link(words[i], words[j]);
            }
        }
    }

    for (auto &kv : graph.word_nodes) {
        std::cout << kv.first << "\n- left: ";
        for (auto &w_node_cnt : kv.second->lefts) {
            std::cout << w_node_cnt.first->word_name << ":" << w_node_cnt.second << " ";
        }
        std::cout << "\n- right: ";
        for (auto &w_node_cnt : kv.second->rights) {
            std::cout << w_node_cnt.first->word_name << ":" << w_node_cnt.second << " ";
        }
        std::cout << std::endl;
    }

    return 0;
}