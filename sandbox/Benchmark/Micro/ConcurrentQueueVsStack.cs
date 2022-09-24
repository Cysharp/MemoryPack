using System.Collections.Concurrent;

namespace Benchmark.Micro;

public class ConcurrentQueueVsStack
{
    ConcurrentQueue<MyClass> q;
    ConcurrentStack<MyClass> stack;

    public ConcurrentQueueVsStack()
    {
        q = new ConcurrentQueue<MyClass>();
        stack = new ConcurrentStack<MyClass>();

        for (int i = 0; i < 100; i++)
        {
            q.Enqueue(new MyClass());
            stack.Push(new MyClass());
        }
    }


    [Benchmark(Baseline = true)]
    public void Queue()
    {
        if (q.TryDequeue(out var v))
        {
            q.Enqueue(v);
        }
    }

    [Benchmark]
    public void Stack()
    {
        if (stack.TryPop(out var v))
        {
            stack.Push(v);
        }
    }

    public class MyClass
    {

    }
}
