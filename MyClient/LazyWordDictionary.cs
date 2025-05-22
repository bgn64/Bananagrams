namespace MyClient
{
    public class LazyWordDictionary
    {
        private readonly LazyTreeNode root;

        public LazyWordDictionary(IEnumerable<string> words)
        {
            List<string> wordList = words.Select(w => w.ToUpperInvariant()).Distinct().ToList();
            root = new LazyTreeNode("", wordList);
        }

        public List<string> GetWordsFromCharacters(List<char> characters)
        {
            List<char> upperChars = characters.Select(char.ToUpperInvariant).ToList();
            HashSet<string> results = new HashSet<string>();
            root.GetWordsFromCharacters(upperChars, results);
            return results.OrderBy(w => w).ToList();
        }

        public void Print()
        {
            root.PrintTree();
        }
    }
}
