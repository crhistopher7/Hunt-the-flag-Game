using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaseReader : MonoBehaviour
{
    public string dataBaseName = "CaseDataBase.txt";
    public char splitter = ';';

    // Start is called before the first frame update
    void Start()
    {
        var database = ReadDataBase();
    }

    private IEnumerable<string> ReadDataBase()
    {
        var file = File.ReadLines(dataBaseName);

        return file;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
