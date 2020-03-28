using System;
using System.Runtime.InteropServices;
using System.Text;

/* ini檔操作類 */

public class INIOperationClass
{


    /*
    * 針對INI檔的API操作方法，其中的節點（Section)、鍵（KEY）都不區分大小寫
    * 如果指定的INI檔不存在，會自動創建該檔。
    * 
    * CharSet定義的時候使用了什麼類型，在使用相關方法時必須要使用相應的類型
    * 例如 GetPrivateProfileSectionNames聲明為CharSet.Auto,那麼就應該使用 Marshal.PtrToStringAuto來讀取相關內容
    * 如果使用的是CharSet.Ansi，就應該使用Marshal.PtrToStringAnsi來讀取內容
    *      
    */

    #region API聲明

    //<summary>
    //獲取所有節點名稱(Section)
    /// </summary>
    //<param name = "lpszReturnBuffer" > 存放節點名稱的記憶體位址, 每個節點之間用\0分隔</param>
    //<param name = "nSize" > 記憶體大小(characters) </ param >
    //< param name="lpFileName">Ini檔</param>
    //<returns>內容的實際長度,為0表示沒有內容,為nSize-2表示記憶體大小不夠</returns>

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

    /// <summary>
    /// 獲取某個指定節點(Section)中所有KEY和Value
    /// </summary>
    /// <param name="lpAppName">節點名稱</param>
    /// <param name="lpReturnedString">返回值的內存地址,每個之間用\0分隔</param>
    /// <param name="nSize">內存大小(characters)</param>
    /// <param name="lpFileName">Ini文件</param>
    /// <returns>內容的實際長度,為0表示沒有內容,為nSize-2表示內存大小不夠</returns>
    /// 
    [DllImport("kernel32.dll",CharSet = CharSet.Auto)]
    private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

    /// <summary>
    /// 讀取INI文件中指定的Key的值
    /// </summary>
    /// <param name="lpAppName">節點名稱。如果為null,則讀取INI中所有節點名稱,每個節點名稱之間用\0分隔</param>
    /// <param name="lpKeyName">Key名稱。如果為null,則讀取INI中指定節點中的所有KEY,每個KEY之間用\0分隔</param>
    /// <param name="lpDefault">讀取失敗時的默認值</param>
    /// <param name="lpReturnedString">讀取的內容緩衝區，讀取之後，多餘的地方使用\0填充</param>
    /// <param name="nSize">內容緩衝區的長度</param>
    /// <param name="lpFileName">INI文件名</param>
    /// <returns>實際讀取到的長度</returns>
    [DllImport("kernel32.dll",CharSet = CharSet.Auto)]
    private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [In, Out] char[] lpReturnedString, uint nSize, string lpFileName);

    //另一種聲明方式,使用 StringBuilder 作為緩衝區類型的缺點是不能接受\0字符，會將\0及其後的字符截斷,
    //所以對於lpAppName或lpKeyName為null的情況就不適用
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

    //再一種聲明，使用string作為緩衝區的類型同char[]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString, uint nSize, string lpFileName);

    /// <summary>
    /// 將指定的鍵值對寫到指定的節點，如果已經存在則替換。
    /// </summary>
    /// <param name="lpAppName">節點，如果不存在此節點，則創建此節點</param>
    /// <param name="lpString">Item鍵值對，多個用\0分隔,形如key1=value1\0key2=value2
    /// <para>如果為string.Empty，則刪除指定節點下的所有內容，保留節點</para>
    /// <para>如果為null，則刪除指定節點下的所有內容，並且刪除該節點</para>
    /// </param>
    /// <param name="lpFileName">INI文件</param>
    /// <returns>是否成功寫入</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]     //可以没有此行
    private static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

    /// <summary>
    /// 將指定的鍵和值寫到指定的節點，如果已經存在則替換
    /// </summary>
    /// <param name="lpAppName">節點名稱</param>
    /// <param name="lpKeyName">鍵名稱。如果為null，則刪除指定的節點及其所有的項目</param>
    /// <param name="lpString">值內容。如果為null，則刪除指定節點中指定的鍵。</param>
    /// <param name="lpFileName">INI文件</param>
    /// <returns>操作是否成功</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

    #endregion

    #region 封装

    /// <summary>
    /// 讀取INI文件中指定INI文件中的所有節點名稱(Section)
    /// </summary>
    /// <param name="iniFile">Ini文件</param>
    /// <returns>所有節點,沒有內容返回string[0]</returns>
    public static string[] INIGetAllSectionNames(string iniFile)
    {
        uint MAX_BUFFER = 32767;    //默認為32767

        string[] sections = new string[0];      //返回值

        //申請內存
        IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));
        uint bytesReturned = INIOperationClass.GetPrivateProfileSectionNames(pReturnedString, MAX_BUFFER, iniFile);
        if (bytesReturned != 0)
        {
            //讀取指定內存的內容
            string local = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned).ToString();

            //每個節點之間用\0分隔,末尾有一個\0
            sections = local.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        //釋放內存
        Marshal.FreeCoTaskMem(pReturnedString);

        return sections;
    }

    /// <summary>
    /// 獲取INI文件中指定節點(Section)中的所有條目(key=value形式)
    /// </summary>
    /// <param name="iniFile">Ini文件</param>
    /// <param name="section">節點名稱</param>
    /// <returns>指定節點中的所有項目,沒有內容返回string[0]</returns>
    public static string[] INIGetAllItems(string iniFile, string section)
    {
        //返回值形式為 key=value,例如 Color=Red
        uint MAX_BUFFER = 32767;    //默認為32767

        string[] items = new string[0];      //返回值

        //分配內存
        IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

        uint bytesReturned = INIOperationClass.GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, iniFile);

        if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
        {

            string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
            items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        Marshal.FreeCoTaskMem(pReturnedString);     //釋放內存

        return items;
    }

    /// <summary>
    /// 獲取INI文件中指定節點(Section)中的所有條目的Key列表
    /// </summary>
    /// <param name="iniFile">Ini文件</param>
    /// <param name="section">節點名稱</param>
    /// <returns>如果沒有內容,反回string[0]</returns>
    public static string[] INIGetAllItemKeys(string iniFile, string section)
    {
        string[] value = new string[0];
        const int SIZE = 1024 * 10;

        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必須指定節點名稱", "section");
        }

        char[] chars = new char[SIZE];
        uint bytesReturned = INIOperationClass.GetPrivateProfileString(section, null, null, chars, SIZE, iniFile);

        if (bytesReturned != 0)
        {
            value = new string(chars).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }
        chars = null;

        return value;
    }

    /// <summary>
    /// 讀取INI文件中指定KEY的字符串型值
    /// </summary>
    /// <param name="iniFile">Ini文件</param>
    /// <param name="section">節點名稱</param>
    /// <param name="key">鍵名稱</param>
    /// <param name="defaultValue">如果沒此KEY所使用的默認值</param>
    /// <returns>讀取到的值</returns>
    public static string INIGetStringValue(string iniFile, string section, string key, string defaultValue)
    {
        string value = defaultValue;
        const int SIZE = 1024 * 10;

        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必須指定節點名稱", "section");
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("必須指定鍵名稱(key)", "key");
        }

        StringBuilder sb = new StringBuilder(SIZE);
        uint bytesReturned = INIOperationClass.GetPrivateProfileString(section, key, defaultValue, sb, SIZE, iniFile);

        if (bytesReturned != 0)
        {
            value = sb.ToString();
        }
        sb = null;

        return value;
    }

    /// <summary>
    /// 在INI文件中，將指定的鍵值對寫到指定的節點，如果已經存在則替換
    /// </summary>
    /// <param name="iniFile">INI文件</param>
    /// <param name="section">節點，如果不存在此節點，則創建此節點</param>
    /// <param name="items">鍵值對，多個用\0分隔,形如key1=value1\0key2=value2</param>
    /// <returns></returns>
    public static bool INIWriteItems(string iniFile, string section, string items)
    {
        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必须指定节点名称", "section");
        }

        if (string.IsNullOrEmpty(items))
        {
            throw new ArgumentException("必须指定键值对", "items");
        }

        return INIOperationClass.WritePrivateProfileSection(section, items, iniFile);
    }

    /// <summary>
    /// 在INI文件中，指定節點寫入指定的鍵及值。如果已經存在，則替換。如果沒有則創建。
    /// </summary>
    /// <param name="iniFile">INI文件</param>
    /// <param name="section">節點</param>
    /// <param name="key">鍵</param>
    /// <param name="value">值</param>
    /// <returns>操作是否成功</returns>
    public static bool INIWriteValue(string iniFile, string section, string key, string value)
    {
        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必須指定節點名稱", "section");
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("必須指定鍵名稱", "key");
        }

        if (value == null)
        {
            throw new ArgumentException("值不能為null", "value");
        }

        return INIOperationClass.WritePrivateProfileString(section, key, value, iniFile);

    }

    /// <summary>
    /// 在INI文件中，刪除指定節點中的指定的鍵。
    /// </summary>
    /// <param name="iniFile">INI文件</param>
    /// <param name="section">節點</param>
    /// <param name="key">鍵</param>
    /// <returns>操作是否成功</returns>
    public static bool INIDeleteKey(string iniFile, string section, string key)
    {
        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必須指定節點名稱", "section");
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("必須指定鍵名稱", "key");
        }

        return INIOperationClass.WritePrivateProfileString(section, key, null, iniFile);
    }

    /// <summary>
    /// 在INI文件中，刪除指定的節點。
    /// </summary>
    /// <param name="iniFile">INI文件</param>
    /// <param name="section">節點</param>
    /// <returns>操作是否成功</returns>
    public static bool INIDeleteSection(string iniFile, string section)
    {
        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必須指定節點名稱", "section");
        }

        return INIOperationClass.WritePrivateProfileString(section, null, null, iniFile);
    }

    /// <summary>
    /// 在INI文件中，刪除指定節點中的所有內容。
    /// </summary>
    /// <param name="iniFile">INI文件</param>
    /// <param name="section">節點</param>
    /// <returns>操作是否成功</returns>
    public static bool INIEmptySection(string iniFile, string section)
    {
        if (string.IsNullOrEmpty(section))
        {
            throw new ArgumentException("必須指定節點名稱", "section");
        }

        return INIOperationClass.WritePrivateProfileSection(section, string.Empty, iniFile);
    }


    #endregion


}