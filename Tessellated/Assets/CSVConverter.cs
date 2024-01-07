using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CSVConverter
{
    public static int[][] Read2DIntCSV(string file)
    {
        var reader = new StreamReader(file);
        var lines = new List<int[]>();

        while (!reader.EndOfStream)
        {
            lines.Add(reader.ReadLine()!.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray());
        }

        return lines.ToArray();

    }
}