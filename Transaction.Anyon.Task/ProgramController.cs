using System.Diagnostics;

public class ProgramController {
    private int _balance = 0;
    private List<BaseTrasaction> list;

    public ProgramController(List<BaseTrasaction> transactions, int balance) {
        list = transactions;
        _balance = balance;
    }

    public List<ProcessedTransaction> Run() {
        return ProcessTransactions(list);
    }


    List<ProcessedTransaction> ProcessTransactions(List<BaseTrasaction> list) {
        var transactions = GetSortById(list);

        return CarryBasicProcessing(transactions);
    }

    List<BaseTrasaction> GetSortById(List<BaseTrasaction> list) {
        return list.OrderBy(x => x.Id).ToList();
    }


    public List<ProcessedTransaction> CarryBasicProcessing(List<BaseTrasaction> transactions) {
        List<TransactionWithSignsValidity> list = new List<TransactionWithSignsValidity>();
        list = ReplicationValidation(transactions);
        list = BalanceValidation(list, _balance);
        list = DuplicateOrderIdValidation(list);

        List<ProcessedTransaction> processedTransactions = new List<ProcessedTransaction>();
        foreach (var transaction in list) {
            if (
                transaction.BalanceValid &&
                transaction.OrderIdValid &&
                transaction.TransactionIdValid) {
                var tempTransaction = new ProcessedTransaction(transaction);
                tempTransaction.Valided = true;
                processedTransactions.Add(tempTransaction);
            }
            else {
                var tempTransaction = new ProcessedTransaction(transaction);
                tempTransaction.Valided = false;
                processedTransactions.Add(tempTransaction);
            }
        }

        return processedTransactions;
    }

    private List<TransactionWithSignsValidity> DuplicateOrderIdValidation(List<TransactionWithSignsValidity> inList) {
        List<TransactionWithSignsValidity> list = inList;
        var duplicateTransactions = list.GroupBy(x => x.OrderId)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.ToList())
            .ToList();
        foreach (var transaction in duplicateTransactions) {
            if (!transaction.OrderIdValid) {
                var transactionsWithSameOrderId = list.Where(x => x.OrderId == transaction.OrderId);
                foreach (var item in transactionsWithSameOrderId) {
                    item.TransactionIdValid = false;
                }
            }
        }

        return list;
    }

    public List<TransactionWithSignsValidity> ReplicationValidation(List<BaseTrasaction> transactions) {
        List<TransactionWithSignsValidity> list = new List<TransactionWithSignsValidity>();
        foreach (var item in transactions) {
            var transactionWithSignsValidity = new TransactionWithSignsValidity();
            transactionWithSignsValidity.SetFields(item);
            list.Add(transactionWithSignsValidity);
        }

        var duplicateTransactions = list.GroupBy(x => x.Id)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.ToList())
            .ToList();
        var firstDuplicateTransactions = duplicateTransactions.GroupBy(x => x.Id)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderBy(x => x.Id).Take(1))
            .ToList();
        foreach (var item in firstDuplicateTransactions) {
            item.OrderIdValid = false;
        }

        return list;
    }

    private List<TransactionWithSignsValidity> BalanceValidation(List<TransactionWithSignsValidity> transactions,
        int balance) {
        List<TransactionWithSignsValidity> list = new List<TransactionWithSignsValidity>();
        foreach (var item in transactions) {
            if (item.TxType == "Bet") {
                balance -= item.Amount;
                if (balance < 0) {
                    balance += item.Amount;
                    item.BalanceValid = false;
                }
            }
            else if (item.TxType == "Win") {
                balance += item.Amount;
                item.BalanceValid = true;
            }
        }

        return list;
    }
}


public class TransactionWithSignsValidity : BaseTrasaction {
    public bool BalanceValid { get; set; }
    public bool OrderIdValid { get; set; }
    public bool TransactionIdValid { get; set; }

    public void SetFields(BaseTrasaction baseTransaction) {
        Id = baseTransaction.Id;
        OrderId = baseTransaction.OrderId;
        Amount = baseTransaction.Amount;
        TxType = baseTransaction.TxType;
    }
}

public class ProcessedTransaction : BaseTrasaction {
    public ProcessedTransaction(TransactionWithSignsValidity item) {
        Id = item.Id;
        OrderId = item.OrderId;
        Amount = item.Amount;
        TxType = item.TxType;
    }

    public bool Valided { get; set; }
}

public class BaseTrasaction {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Amount { get; set; }
    public string TxType { get; set; }
}