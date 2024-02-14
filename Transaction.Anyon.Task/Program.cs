using System.Diagnostics;

public class Program {
    private int _balance = 0;

    public void Main(string[] args) {
        List<BaseTrasaction> list = new List<BaseTrasaction>()
        {
            new BaseTrasaction()
            {
                Id = 1,
                OrderId = 1,
                Amount = 100,
                TxType = "Bet",
            },
            new BaseTrasaction()
            {
                Id = 2,
                OrderId = 2,
                Amount = 10,
                TxType = "Win",
            },
            new BaseTrasaction()
            {
                Id = 3,
                OrderId = 3,
                Amount = 1000,
                TxType = "Bet",
            },
        };

        var processedTransactions = ProcessTransactions(list);

        List<ProcessedTransaction> ProcessTransactions(List<BaseTrasaction> list) {
            var transactions = GetSortById(list);

            return CarryBasicProcessing(transactions);
        }

        List<BaseTrasaction> GetSortById(List<BaseTrasaction> list) {
            return list.OrderBy(x => x.Id).ToList();
        }
    }

    public List<ProcessedTransaction> CarryBasicProcessing(List<BaseTrasaction> transactions) {
        List<ProcessedTransaction> processedTransactions = new List<ProcessedTransaction>();
        (List<BaseTrasaction> validTransactions, List<ProcessedTransaction> invalidTransactions) =
            GetProsessingById(transactions);
        processedTransactions.AddRange(invalidTransactions);
        processedTransactions.AddRange(GetValidityFilterByBalance(validTransactions));
        processedTransactions = GetValidationByOrderId(processedTransactions);


        return processedTransactions;
    }

    public ( List<BaseTrasaction> validTransactions,
        List<ProcessedTransaction> invalidTransactions) GetProsessingById(List<BaseTrasaction> transactions) {
        List<BaseTrasaction> validTransactions = new List<BaseTrasaction>();
        List<ProcessedTransaction> invalidTransactions = new List<ProcessedTransaction>();
        List<BaseTrasaction> selectedTransactions = FindTransactionsWithSameId(transactions);
    }

    public List<BaseTrasaction> FindTransactionsWithSameId(List<BaseTrasaction> transactions) {
        List<int> uniqueIds = transactions.Select(t => t.Id).Distinct().ToList();
        return GetTransactionsWithSameId(transactions, uniqueIds);
    }

    private List<BaseTrasaction> GetTransactionsWithSameId(List<BaseTrasaction> transactions, List<int> uniqueIds) {
        return transactions.Where(t => uniqueIds.Contains(t.Id)).ToList();
    }


    private List<ProcessedTransaction> GetValidationByOrderId(List<ProcessedTransaction> transactions) {
        var groupedTransactions = transactions.GroupBy(x => x.OrderId);
        transactions = groupedTransactions.SelectMany(group =>
            group.Select(item => new ProcessedTransaction()
            {
                Id = item.Id,
                OrderId = item.OrderId,
                Amount = item.Amount,
                TxType = item.TxType,
                Valided = group.Count() == 1
            })
        ).ToList();

        return transactions;
    }

    public List<ProcessedTransaction> GetValidityFilterByBalance(List<BaseTrasaction> transactions) {
        List<ProcessedTransaction> processedTransactions = new List<ProcessedTransaction>();
        foreach (var item in transactions) {
            (string TxType, bool Valided) balanceProccessing = BalanceProcessioning(item.TxType, item.Amount);
            processedTransactions.Add(
                new ProcessedTransaction()
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    Amount = item.Amount,
                    TxType = balanceProccessing.TxType,
                    Valided = balanceProccessing.Valided,
                }
            );
        }

        (string TxType, bool Valided) BalanceProcessioning(string itemTxType, int amount) {
            if (itemTxType == "Bet") {
                _balance -= amount;
            }
            else if (itemTxType == "Win") {
                _balance += amount;
            }

            return (itemTxType, _balance > 0);
        }

        return processedTransactions;
    }
}

public class TransactionWithSignsValidity : BaseTrasaction {
    public bool BalanceValid { get; set; }
    public bool OrderIdValid { get; set; }
    public bool TransactionIdValid { get; set; }
    public bool Processed { get; set; }
}

public class ProcessedTransaction : BaseTrasaction {
    public bool Valided { get; set; }
}

public class BaseTrasaction {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Amount { get; set; }
    public string TxType { get; set; }
}