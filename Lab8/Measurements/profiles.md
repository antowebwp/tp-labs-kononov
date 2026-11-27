# Выборка управляемых стеков на macOS

Трассы записаны dotnet-trace 10.0.750501 в Release. Это отчёт EventPipe, не Visual Studio CPU Usage. Исходная версия — один прогон, оптимизированная — 200 прогонов, поэтому проценты нельзя трактовать как сравнение времени одинакового количества операций.

## До оптимизации

```text
Top 12 Functions (Exclusive)                                                  Inclusive           Exclusive           
1.  SpanHelpers.Memmove(unsigned int8&,unsigned int8&,unsigned int)           37.52%              37.52%              
2.  Baseline.Sort(class Lab8.Optimization.PurchaseData,value class System.    27.1%               27.1%               
3.  String.Concat(value class System.ReadOnlySpan`1<class System.String>)     53.33%              15.81%              
4.  Baseline.Totals(class Lab8.Optimization.PurchaseData,class System.Coll    13.68%              10.88%              
5.  SpanHelpers.SequenceEqual(unsigned int8&,unsigned int8&,unsigned int)     4.05%               4.05%               
6.  Baseline.Unique(class Lab8.Optimization.PurchaseData,value class Syste    4.87%               1.9%                
7.  __Canon].IndexOf(!0[],!0,int32,int32)                                     1.66%               1.26%               
8.  __Canon].Contains(!0)                                                     2.08%               0.41%               
9.  CultureInfo.GetCultureByName(class System.String)                         0.16%               0.16%               
10. Baseline.Report(class Lab8.Optimization.PurchaseData,value class Syste    53.43%              0.11%               
11. BenchmarkEngine.RunProfile(class Lab8.Optimization.PurchaseData,bool,i    99.17%              0.08%               
12. PurchaseData.Generate(int32)                                              0.08%               0.08%               

```

## После оптимизации

```text
Top 12 Functions (Exclusive)                                                  Inclusive           Exclusive           
1.  Int32].PickPivotAndPartition(value class System.Span`1<!0>)               32.7%               32.7%               
2.  Optimized.Totals(class Lab8.Optimization.PurchaseData,class System.Col    28.39%              11.15%              
3.  GC.<AllocateUninitializedArray>g__AllocateNewArrayWorker|77_0(int32,bo    9.44%               9.44%               
4.  Optimized.Unique(class Lab8.Optimization.PurchaseData,value class Syst    16.58%              9.11%               
5.  SpanHelpers.SequenceEqual(unsigned int8&,unsigned int8&,unsigned int)     7.83%               7.83%               
6.  Thread.<PollGC>g__PollGCWorker|67_0()                                     7.76%               7.76%               
7.  Int64].FindValue(!0)                                                      8.12%               5.38%               
8.  Int32].IntroSort(value class System.Span`1<!0>,int32)                     39.41%              3.89%               
9.  Int32].InsertionSort(value class System.Span`1<!0>)                       2.82%               2.82%               
10. NonRandomizedStringEqualityComparer+OrdinalComparer.GetHashCode(class     1.5%                1.5%                
11. Optimized+<>c__DisplayClass3_0.<Totals>b__0(class System.String)          2.59%               1.23%               
12. Optimized.Report(class Lab8.Optimization.PurchaseData,value class Syst    10.88%              0.93%               

```

Исходный профиль показывает затраты на перемещение памяти/конкатенацию строк, Baseline.Sort, Baseline.Totals и поиск уникальных покупателей (Baseline.Unique/List.Contains). Отдельные замеры всех четырёх участков приведены в README и reference.json.
