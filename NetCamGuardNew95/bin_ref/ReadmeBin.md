# 軟件鎖的用法

**引用經過混淆處裡的文件 Encryption.dll**



```
例如 文件中 NetCamGuardNew95/VxClient1\Program.cs
//當運行發佈版本的時候,判斷是否有Liciense 文件 AppAuth.key
//放在Bin目錄下,如果沒有則到C 低下尋找
#if RELEASE
                    bool v = true; //EncryptionRSA.VerifyCurrentMachine(); //true;//
                    v = EncryptionRSA.VerifyCurrentMachine(); 
                    Console.WriteLine("CHECK ERSA = {0}",v); 
#endif

```

