using System;

class Program {
    enum Status { Pending, Active }
    static void Main() {
        try {
            string? s = null;
            bool result = Enum.TryParse<Status>(s, true, out var parsed);
            Console.WriteLine($"Result: {result}, Parsed: {parsed}");
        } catch (Exception ex) {
            Console.WriteLine($"Exception: {ex.GetType().Name}");
        }
    }
}
