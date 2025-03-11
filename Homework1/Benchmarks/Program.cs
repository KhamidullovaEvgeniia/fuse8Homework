using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Fuse8.BackendInternship.Domain;

BenchmarkRunner.Run<AccountProcessorBenchmark>();

// Комментарий к бэнчмарку StringInternBenchmark
// При сравнении работы методов WordIsExists и WordIsExistsIntern можно заметить, что при поиске первого слова в словаре
// метод WordIsExists быстрее (23.4 нс и 55.56 нс соответсвенно). Это связано с тем, что в первом методе происходит
// побитовое сравнение строк в методе Equals, поэтому для первого элемента метод сразу возвращает true,
// а во втором некоторое время тратится на интернирование строки.
// В остальных случаях более оптимальным по времени является метод WordIsExistsIntern.
// Однако разница во времени не более 10 - 20%.
// Такая небольшая разница предположительно объясняется тем,
// что в методе Equals происходит ускорение за счет сравнения длин строк.

[MemoryDiagnoser(displayGenColumns: true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class StringInternBenchmark
{
    private readonly List<string> _words = new();

    public StringInternBenchmark()
    {
        foreach (var word in File.ReadLines(@".\SpellingDictionaries\ru_RU.dic"))
            _words.Add(string.Intern(word));
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(SampleData))]
    public bool WordIsExists(string word) => _words.Any(item => word.Equals(item, StringComparison.Ordinal));

    [Benchmark]
    [ArgumentsSource(nameof(SampleData))]
    public bool WordIsExistsIntern(string word)
    {
        var internedWord = string.Intern(word);
        return _words.Any(item => ReferenceEquals(internedWord, item));
    }

    public IEnumerable<string> SampleData()
    {
        yield return _words[0];
        yield return _words[_words.Count / 2];
        yield return _words[_words.Count - 1];
        yield return "лывоатыл";

        yield return new StringBuilder("соль").ToString();
        yield return new StringBuilder("перец").ToString();
        yield return new StringBuilder("фывафы").ToString();
    }
}

[MemoryDiagnoser(displayGenColumns: true)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class AccountProcessorBenchmark
{
    private readonly AccountProcessor _processor = new();

    private BankAccount _bankAccount = new();

    [Benchmark(Baseline = true)]
    public decimal Calculate() => _processor.Calculate(_bankAccount);

    [Benchmark]
    public decimal CalculatePerformed() => _processor.CalculatePerformed(ref _bankAccount);

    public AccountProcessorBenchmark()
    {
        _bankAccount = new BankAccount
        {
            TotalAmount = 1000,
            LastOperation = new BankOperation
            {
                OperationInfo0 = 10,
                OperationInfo1 = 20,
                OperationInfo2 = 30,
                TotalAmount = 100
            },
            PreviousOperation = new BankOperation
            {
                OperationInfo0 = 40,
                OperationInfo1 = 50,
                OperationInfo2 = 60,
                TotalAmount = 200
            }
        };
    }
}