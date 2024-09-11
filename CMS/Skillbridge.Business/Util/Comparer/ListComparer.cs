namespace SkillBridge.Business.Util.Comparer;

public class StringComparer : IComparer<string>
{
    public int Compare(string a, string b)
    {
        string[] arr1 = a.Split(' ');
        string[] arr2 = b.Split(' ');
        string str1 = arr1[0];
        string str2 = arr2[0];
        return str1.CompareTo(str2);
    }
}

public class NumberComparer : IComparer<string>
{
    public int Compare(string a, string b)
    {
        string[] arr1 = a.Split(' ');
        string[] arr2 = b.Split(' ');
        int int1 = int.Parse(arr1[1]);
        int int2 = int.Parse(arr2[1]);
        return int1.CompareTo(int2);
    }
}