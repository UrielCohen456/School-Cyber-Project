using System.Collections.Generic;

namespace DataLayer
{
    public interface IWordsRepository
    {
        IEnumerable<Word> GetRandomWords(int wordCount = 20);
    }

    public class WordsRepository : IWordsRepository
    {
        private readonly IDb<Word> Db;

        public WordsRepository(IDb<Word> db)
        {
            Db = db;
        }

        public IEnumerable<Word> GetRandomWords(int wordCount = 20)
        {
            var words = Db.Select(wordCount, "order by newid()");

            return words;
        }
    }
}
