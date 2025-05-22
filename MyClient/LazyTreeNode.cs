namespace MyClient
{
    public class LazyTreeNode
    {
        private readonly string prefix;
        private readonly List<string> wordsWithPrefix;
        private readonly Dictionary<char, Lazy<LazyTreeNode?>> children;

        public bool IsWord { get; }

        public LazyTreeNode(string prefix, List<string> wordsWithPrefix)
        {
            this.prefix = prefix;
            this.wordsWithPrefix = wordsWithPrefix;
            IsWord = this.wordsWithPrefix.Contains(this.prefix);

            children = new Dictionary<char, Lazy<LazyTreeNode?>>();
            IEnumerable<char> nextChars = wordsWithPrefix
            .Where(w => w.Length > prefix.Length)
            .Select(w => w[prefix.Length])
            .Distinct();

            foreach (char ch in nextChars)
            {
                children[ch] = new Lazy<LazyTreeNode?>(() =>
                {
                    string newPrefix = this.prefix + ch;
                    List<string> filteredWords = wordsWithPrefix
                    .Where(w => w.StartsWith(newPrefix))
                    .ToList();

                    return filteredWords.Count > 0 ? new LazyTreeNode(newPrefix, filteredWords) : null;
                });
            }
        }

        public LazyTreeNode? GetChild(char ch)
        {
            return children.TryGetValue(ch, out Lazy<LazyTreeNode?>? lazyNode) ? lazyNode.Value : null;
        }

        public void GetWordsFromCharacters(List<char> chars, HashSet<string> results)
        {
            if (IsWord)
                results.Add(prefix);

            HashSet<char> used = new HashSet<char>();
            for (int i = 0; i < chars.Count; i++)
            {
                char ch = chars[i];
                if (used.Contains(ch)) continue;
                used.Add(ch);

                LazyTreeNode? child = GetChild(ch);
                if (child != null)
                {
                    List<char> remaining = new List<char>(chars);
                    remaining.RemoveAt(i);
                    child.GetWordsFromCharacters(remaining, results);
                }
            }
        }

        public void PrintTree(string indent = "", char? branchChar = null)
        {
            string label = branchChar.HasValue ? $"{branchChar} => " : "";
            Console.WriteLine($"{indent}{label}{prefix}{(IsWord ? " [word]" : "")}");

            foreach (KeyValuePair<char, Lazy<LazyTreeNode?>> kvp in children.OrderBy(k => k.Key))
            {
                char ch = kvp.Key;
                Lazy<LazyTreeNode?> lazyNode = kvp.Value;

                if (!lazyNode.IsValueCreated)
                {
                    Console.WriteLine($"{indent}  {ch} => [not yet initialized]");
                }
                else if (lazyNode.Value == null)
                {
                    Console.WriteLine($"{indent}  {ch} => null [no words]");
                }
                else
                {
                    lazyNode.Value.PrintTree(indent + "  ", ch);
                }
            }
        }
    }
}
