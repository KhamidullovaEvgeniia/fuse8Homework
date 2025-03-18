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
// | Method               | word          | Mean            | Error         | StdDev        | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
// |----------------------|---------------|-----------------|---------------|---------------|-------|--------|------|--------|----------  |------------|
// | WordIsExists         | 146269        |        23.40 ns |      0.414 ns |      0.367 ns |  1.00 |   0.00 |    1 | 0.0153 |      128 B |       1.00 |
// | WordIsExistsIntern   | 146269        |        55.56 ns |      0.615 ns |      0.576 ns |  2.38 |   0.05 |    2 | 0.0153 |      128 B |       1.00 |
// |                      |               |                 |               |               |       |        |      |        |            |            |             
// | WordIsExistsIntern   | ёкающий/A     | 1,000,176.83 ns |  8,292.935 ns |  6,924.976 ns |  0.84 |   0.01 |    1 |      - |      129 B |       1.00 |
// | WordIsExists         | ёкающий/A     | 1,189,429.52 ns |  9,143.925 ns |  7,635.592 ns |  1.00 |   0.00 |    2 |      - |      129 B |       1.00 |
// |                      |               |                 |               |               |       |        |      |        |            |            |
// | WordIsExistsIntern   | львоатль      | 1,028,177.57 ns |  7,369.357 ns |  6,893.301 ns |  0.88 |   0.02 |    1 |      - |      129 B |       1.00 |
// | WordIsExists         | львоатль      | 1,162,835.96 ns | 15,940.019 ns | 14,910.303 ns |  1.00 |   0.00 |    2 |      - |      129 B |       1.00 |
// |                      |               |                 |               |               |       |        |      |        |            |            |
// | WordIsExistsIntern   | перец         | 1,022,870.60 ns |  6,566.110 ns |  5,482.999 ns |  0.92 |   0.01 |    1 |      - |      129 B |       1.00 |
// | WordIsExists         | перец         | 1,111,106.07 ns |  2,396.804 ns |  1,871.267 ns |  1.00 |   0.00 |    2 |      - |      129 B |       1.00 |
// |                      |               |                 |               |               |       |        |      |        |            |            |
// | WordIsExistsIntern   | полиморфизм/J |   496,540.73 ns |  3,317.607 ns |  2,940.972 ns |  0.79 |   0.01 |    1 |      - |     128 B  |       1.00 |
// | WordIsExists         | полиморфизм/J |   627,455.26 ns |  6,291.814 ns |  5,885.367 ns |  1.00 |   0.00 |    2 |      - |      128 B |       1.00 |
// |                      |               |                 |               |               |       |        |      |        |            |            |
// | WordIsExistsIntern   | соль          | 1,020,671.41 ns |  7,217.593 ns |  6,751.341 ns |  0.92 |   0.01 |    1 |      - |      129 B |       1.00 |
// | WordIsExists         | соль          | 1,109,825.20 ns | 11,075.307 ns |  9,817.971 ns |  1.00 |   0.00 |    2 |      - |      129 B |       1.00 |
// |                      |               |                 |               |               |       |        |      |        |            |            |
// | WordIsExistsIntern   | фывафы        | 1,022,827.43 ns |  9,529.362 ns |  8,447.531 ns |  0.90 |   0.01 |    1 |      - |      129 B |       1.00 |
// | WordIsExists         | фывафы        | 1,131,197.42 ns | 18,596.128 ns | 17,394.829 ns |  1.00 |   0.00 |    2 |      - |      129 B |       1.00 |

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

// Комментарий к бэнчмарку AccountProcessorBenchmark
// При реализации метода CalculatePerformed без копирования и боксинга время, затраченное на выполнение кода в 4 раза меньше, 
// чем при выполнении Calculate. Также из таблицы видно, что при в методе CalculatePerformed не выделяется память в куче.
// Таким образом, метод без боксинга и копирования оптимальнее за счет уменьшения времени и отсуствия выделения памяти в куче.
// |             Method |     Mean |    Error |   StdDev | Ratio | Rank |   Gen0 | Allocated | Alloc Ratio |
// |------------------- |---------:|---------:|---------:|------:|-----:|-------:|----------:|------------:|
// | CalculatePerformed | 215.3 ns |  4.26 ns |  8.01 ns |  0.26 |    1 |      - |         - |        0.00 |
// |          Calculate | 838.1 ns | 16.37 ns | 16.08 ns |  1.00 |    2 | 2.1420 |    6720 B |        1.00 |


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