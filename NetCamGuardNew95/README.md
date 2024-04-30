# AI GUARD 開發說明文檔

## VxClient.xml文件

```
File name: '\VxClient1\bin\Debug\netcoreapp3.1\VxClient.xml'
注意:第一次啟動需要把文件複製到bin運行目錄下,這是 SwaggerUI API組件必須的
```

## 數據庫密碼

//admin
//admin62595738     MD5 32 UPCASE : D05751D442A016B5D03AE3BE3B1A37D3

BackEnd 賬號密碼

admin2 密碼 admin123

## 數據庫

數據庫可以是通過腳本生成,也可以通過DatabaseCreate項目生成

處理404與401
發佈版處理404(由於內部錯誤)和401等等問題,有時候會混淆一起提示 

asp.net core 自定义401和异常显示内容（JWT认证、Cookie Base认证失败显示内容）

未能出来401问题
https://blog.csdn.net/weixin_30484739/article/details/97040792?utm_medium=distribute.pc_relevant_bbs_down.none-task-blog-baidujs-1.nonecase&depth_1-utm_source=distribute.pc_relevant_bbs_down.none-task-blog-baidujs-1.nonecase

## 實現多公司雲架構

實現雲架構, 主要通過 Library 表加入 公司主ID (MainComID) 實現對 全局查詢. 
		在Library加入MainComId ,有幾大好處:最節省的方式實現架構改造.
		Library ID (底庫) 在person表, Hist record 表都有用到. 但需要間接查詢,例如先通過MainComID 查Library在去查Person表.
		後續 在設備表FtDevice\鏡頭表FtCamera 都會加入MainComId,

## 本地與互聯網交互播放

在Media.exe運行後,產生的HLS本地與 VxClient雲端互聯網的交互播放問題

关于播放器不能交互播放本地 IPC (ip camera),主要是Chrome瀏覽器的PLA規則問題.
Uncaught (in promise) DOMException: play() failed because the user didn't interact with the document first.
Uncaught (in promise) DOMException: play() failed 因為用戶沒有先與文檔交互。

命令： about://media-engagement
列出chrome浏览器启动的媒体播放功能设置

启动自动播放  设置教程：█★★★★★ https://bbs.huaweicloud.com/blogs/312066
You can disable the autoplay policy entirely by using a command line flag: 

chrome.exe --autoplay-policy=no-user-gesture-required. 

This allows you to test your website as if user were strongly engaged with your site and playback autoplay would be always allowed.

不允许自动播放
You can also decide to make sure autoplay is never allowed by disabling MEI and whether sites with the highest overall MEI get autoplay by default for new users. 
Do this with flags: chrome.exe --disable-features=PreloadMediaEngagementData, MediaEngagementBypassAutoplayPolicies.

浏览器标记：
chrome://flags/  配置具体的项目
 .m3u8    application/x-mpegURL
 .ts  video/MP2T

 SignalR 引入 Microsoft.AspNet.SignalR Version 2.4.3
 https://docs.microsoft.com/zh-tw/aspnet/core/signalr/introduction?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0
 ref https://www.freesion.com/article/30591185625/

 https://docs.microsoft.com/zh-tw/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-6.0&tabs=visual-studio
 https://docs.microsoft.com/zh-tw/aspnet/core/tutorials/signalr?WT.mc_id=dotnet-35129-website&view=aspnetcore-3.1&tabs=visual-studio
 dotnet dev-certs https --clean
dotnet dev-certs https --trust