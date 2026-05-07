using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class BinomialNode
{
    public int Key;
    public int Degree;
    public BinomialNode Parent;
    public BinomialNode Child;
    public BinomialNode Sibling;

    public BinomialNode(int key)
    {
        Key = key;
    }
}

class BinomialHeap
{
    public BinomialNode Head;
    public long Operations = 0;

    private BinomialNode MergeRoots(BinomialNode h1, BinomialNode h2)
    {
        Operations++;
        if (h1 == null) return h2;
        if (h2 == null) return h1;

        BinomialNode head;
        BinomialNode tail;

        if (h1.Degree <= h2.Degree)
        {
            head = h1;
            h1 = h1.Sibling;
        }
        else
        {
            head = h2;
            h2 = h2.Sibling;
        }

        tail = head;

        while (h1 != null && h2 != null)
        {
            Operations++;
            if (h1.Degree <= h2.Degree)
            {
                tail.Sibling = h1;
                h1 = h1.Sibling;
            }
            else
            {
                tail.Sibling = h2;
                h2 = h2.Sibling;
            }
            tail = tail.Sibling;
        }

        tail.Sibling = h1 ?? h2;
        return head;
    }

    private void LinkTrees(BinomialNode y, BinomialNode z)
    {
        Operations++;
        y.Parent = z;
        y.Sibling = z.Child;
        z.Child = y;
        z.Degree++;
    }

    public void Union(BinomialHeap other)
    {
        Head = MergeRoots(this.Head, other.Head);
        if (Head == null) return;

        BinomialNode prev = null;
        BinomialNode curr = Head;
        BinomialNode next = curr.Sibling;

        while (next != null)
        {
            Operations++;

            if (curr.Degree != next.Degree ||
               (next.Sibling != null && next.Sibling.Degree == curr.Degree))
            {
                prev = curr;
                curr = next;
            }
            else
            {
                if (curr.Key <= next.Key)
                {
                    curr.Sibling = next.Sibling;
                    LinkTrees(next, curr);
                }
                else
                {
                    if (prev == null)
                        Head = next;
                    else
                        prev.Sibling = next;

                    LinkTrees(curr, next);
                    curr = next;
                }
            }
            next = curr.Sibling;
        }
    }

    public void Insert(int key)
    {
        BinomialHeap temp = new BinomialHeap();
        temp.Head = new BinomialNode(key);
        Union(temp);
    }

    public int GetMin()
    {
        Operations++;
        if (Head == null) throw new Exception("пуста");

        int min = int.MaxValue;
        BinomialNode curr = Head;

        while (curr != null)
        {
            Operations++;
            if (curr.Key < min)
                min = curr.Key;

            curr = curr.Sibling;
        }

        return min;
    }

    public int ExtractMin()
    {
        if (Head == null) throw new Exception("пуста");

        BinomialNode minNode = Head;
        BinomialNode minPrev = null;

        BinomialNode curr = Head;
        BinomialNode prev = null;

        int min = curr.Key;

        while (curr != null)
        {
            Operations++;
            if (curr.Key < min)
            {
                min = curr.Key;
                minNode = curr;
                minPrev = prev;
            }
            prev = curr;
            curr = curr.Sibling;
        }

        if (minPrev != null)
            minPrev.Sibling = minNode.Sibling;
        else
            Head = minNode.Sibling;

        BinomialNode child = minNode.Child;
        BinomialNode rev = null;

        while (child != null)
        {
            Operations++;
            BinomialNode next = child.Sibling;
            child.Sibling = rev;
            child.Parent = null;
            rev = child;
            child = next;
        }

        BinomialHeap temp = new BinomialHeap();
        temp.Head = rev;

        Union(temp);

        return min;
    }

    public bool Find(int key)
    {
        return FindNode(Head, key) != null;
    }

    private BinomialNode FindNode(BinomialNode node, int key)
    {
        while (node != null)
        {
            Operations++;

            if (node.Key == key)
                return node;

            var res = FindNode(node.Child, key);
            if (res != null) return res;

            node = node.Sibling;
        }
        return null;
    }

    public void Delete(int key)
    {
        if (!Find(key)) return;
        DecreaseKey(key, int.MinValue);
        ExtractMin();
    }

    private void DecreaseKey(int oldKey, int newKey)
    {
        var node = FindNode(Head, oldKey);
        if (node == null) return;

        node.Key = newKey;

        while (node.Parent != null && node.Key < node.Parent.Key)
        {
            Operations++;
            int temp = node.Key;
            node.Key = node.Parent.Key;
            node.Parent.Key = temp;

            node = node.Parent;
        }
    }
}

class Program
{
    static void Main()
    {
        Random rand = new Random();

        int[] data = new int[10000];
        for (int i = 0; i < data.Length; i++)
            data[i] = rand.Next(0, 100000);

        BinomialHeap heap = new BinomialHeap();

        List<long> insertOps = new();
        List<long> findOps = new();
        List<long> deleteOps = new();

        List<long> insertTime = new();
        List<long> findTime = new();
        List<long> deleteTime = new();

        Stopwatch sw = new Stopwatch();

        // вставко
        foreach (var x in data)
        {
            heap.Operations = 0;
            sw.Restart();

            heap.Insert(x);

            sw.Stop();
            insertOps.Add(heap.Operations);
            insertTime.Add(sw.ElapsedTicks);
        }

        // поиск 100 элементов
        var searchSample = data.OrderBy(x => rand.Next()).Take(100);

        foreach (var x in searchSample)
        {
            heap.Operations = 0;
            sw.Restart();

            heap.Find(x);

            sw.Stop();
            findOps.Add(heap.Operations);
            findTime.Add(sw.ElapsedTicks);
        }

        // удаление 1000 элементов
        var deleteSample = data.OrderBy(x => rand.Next()).Take(1000);

        foreach (var x in deleteSample)
        {
            heap.Operations = 0;
            sw.Restart();

            heap.Delete(x);

            sw.Stop();
            deleteOps.Add(heap.Operations);
            deleteTime.Add(sw.ElapsedTicks);
        }

        Console.WriteLine("средние значения");

        Console.WriteLine($"вставка - операции: {insertOps.Average()}");
        Console.WriteLine($"вставка - время: {insertTime.Average()}");

        Console.WriteLine($"поиск - операции: {findOps.Average()}");
        Console.WriteLine($"поиск - время: {findTime.Average()}");

        Console.WriteLine($"удаление - операции: {deleteOps.Average()}");
        Console.WriteLine($"удаление - время: {deleteTime.Average()}");
    }
}