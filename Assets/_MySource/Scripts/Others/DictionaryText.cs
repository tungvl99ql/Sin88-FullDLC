using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryText{
    public static DictionaryText instance;
    private Dictionary<int, string> dicString = new Dictionary<int, string>();
    public DictionaryText()
    {
        Init();
    }
    public static DictionaryText GetInstance()
    {
        if (instance == null)
        {
            instance = new DictionaryText();
        }
        return instance;
    }


    private void Init()
    {
        dicString.Add(1, "Hệ thống tự động chọn thẻ bài ngẫu nhiên : ");
        dicString.Add(2, "Đang tự động chọn bài ....");
        dicString.Add(3, "Kết thúc chọn bài Sau : ");
        dicString.Add(4, "Tự động vào mở bài trong : ");

    }

    public string TryGetValue(int _key)
    {
        string value;
        if (dicString.TryGetValue(_key, out value))
        {
        }
        else
        {
            value = "Không có giá trị phù hợp với key";
        }
        return value;
    }
}
