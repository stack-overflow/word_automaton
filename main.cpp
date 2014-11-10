#include <iostream>
#include <vector>
#include <map>
#include <fstream>
#include <string>
#include <iterator>
#include <functional>
#include <algorithm>
#include <deque>
#include <sstream>

std::string trim_begin(const std::string& str) {
    auto alpha = std::find_if_not(str.begin(), str.end(), ::isspace);
    return str.substr(std::distance(str.begin(), alpha));
}

std::string trim_end(const std::string& str) {
    auto alpha = std::find_if_not(str.rbegin(), str.rend(), ::isspace).base();
    return str.substr(0, std::distance(str.begin(), alpha));
}

std::string str_to_lower(const std::string& str) {
    std::string lowercase;
    std::transform(std::begin(str), std::end(str), std::back_inserter(lowercase), ::tolower);
    return lowercase;
}

std::vector<std::string> str_split_whitespace(const std::string& str) {
    std::vector<std::string> words;
    std::istringstream is(str);
    std::copy(
        std::istream_iterator<std::string>(is),
        std::istream_iterator<std::string>(),
        std::back_inserter(words));
    return words;
}

std::vector<std::string> str_split(const std::string& str, const char *delims) {
    size_t found = std::string::npos, prev = 0;
    std::vector<std::string> out;
    out.reserve(log(str.size()));

    found = str.find_first_of(delims);
    while (found != std::string::npos) {
        if (prev < found) {
            out.push_back(str.substr(prev, found - prev));
        }
        prev = found + 1;
        found = str.find_first_of(delims, prev);
    }
    out.push_back(str.substr(prev, std::string::npos));

    return out;
}

std::vector<std::string> extract_sentences(const std::string& str) {
    return str_split(str, ".?!");
}

std::vector<std::string> extract_words(const std::string& str) {
    return str_split(str, " .,;\"?!");
}

class sentence_file_reader {
    std::ifstream ifs;
    std::deque<std::string> sentence_buffer;
    static const size_t BUFFER_SIZE = 16 * 1024;
    char char_buffer[BUFFER_SIZE];

public:
    sentence_file_reader(const char *filename) :
        ifs(filename, std::ios::in)
    {}

    ~sentence_file_reader() {
        ifs.close();
    }

    std::string get_next_sentence() {
        std::string sn;
        if (!sentence_buffer.empty()) {
            sn = sentence_buffer.front();
            sentence_buffer.pop_front();
        }
        else {
            ifs.getline(char_buffer, BUFFER_SIZE);
            auto sentences = extract_sentences(char_buffer);
            sentence_buffer = std::deque<std::string>(std::begin(sentences), std::end(sentences));
            sn = get_next_sentence();
        }
        return trim_begin(trim_end(sn));
    }
    bool has_more() {
        return ifs.good() || !sentence_buffer.empty();
    }
};

struct word_node {
    word_node(const std::string& name) :
    original_name(name),
    normalized_name(str_to_lower(name))
    {}

    std::string original_name;
    std::string normalized_name;
    std::map<word_node*, size_t> lefts;
    std::map<word_node*, size_t> rights;
};

struct word_graph {
    std::map<std::string, word_node*> word_nodes;
    word_node *head;

    word_node *get_or_create(std::string name) {
        auto word = str_to_lower(name);
        auto name_exists = word_nodes.equal_range(word);
        if (name_exists.first == name_exists.second) {
            word_node *name_node = new word_node(name);
            return word_nodes.insert(name_exists.second, std::make_pair(word, name_node))->second;
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

int main(int argc, char *argv[]) {
    std::ios::ios_base::sync_with_stdio(false);
    const char *in_filename = "test.txt";

    if (argc > 1) {
        in_filename = argv[1];
    }
    sentence_file_reader cfr(in_filename);

    word_graph graph;
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
            std::cout << w_node_cnt.first->normalized_name << ":" << w_node_cnt.second << " ";
        }
        std::cout << "\n- right: ";
        for (auto &w_node_cnt : kv.second->rights) {
            std::cout << w_node_cnt.first->normalized_name << ":" << w_node_cnt.second << " ";
        }
        std::cout << std::endl;
    }

    return 0;
}
