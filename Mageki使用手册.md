# 使用手册
Mageki可以使用移动设备来连接到[ongeki-io](https://github.com/Sanheiii/ongeki-io)，来到这个页面的你一定已经迫不及待了，但是首先你需要一个能够成功运行并使用其它方式控制的游戏本体。如果还没有，请参阅：
### [最新最热的游戏下载与配置说明](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app)
当游戏本体配置完备后，才可以进行下面的步骤。
### 注意事项：[最新最热的游戏下载与配置说明](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app)
- 适配
  - 本教程仅适用于 [Mageki]([https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app](https://github.com/Sanheiii/Mageki)) 的配置教程，请勿拿别的手台使用Mageki教程并配置，Its Useless。
- 教程中的 [蓝字](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app)
  - 教程中的所有 [蓝字](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app) 都是可以 [点击](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app) 的，如果你Stuck在哪个步骤了，不妨按下这个神奇的蓝色按钮呢？
- 必要性
  - 本教程是Mageki非常必要的一个Getting Start篇章，如果您在初次配置完后发现连接不上，请按照可能遇到的问题开始排查。

## 开始使用
请前往 __[发布页](https://github.com/Sanheiii/Mageki/releases)__ 下载所需的文件。
### 安装程序
- Android
  1. 下载 __mageki_x.x.x.apk__。
  2. 在Android设备上打开以安装程序。
- iOS
  - 进入页面按照提示操作。
- UWP
  - 缺少触屏设备无法调试，如有需求请自行编译，Debug并提交Pull request。
### 配置ongeki-io
1.  __MU3Input_x.x.x.zip__ 的内容解压至 __SDDT\package__ 目录下。
2. 修改 __segatools.ini__ 中 __[mu3io]__ 与 __[aimeio]__ 下的内容，如果没有，则添加它们。
``` ini
[mu3io]
path = MU3Input.dll

[aimeio]
path = MU3Input.dll
```
3. 根据 [__mu3input_config.json说明__](https://github.com/Sanheiii/Mageki/wiki/mu3input_config.json) 中的指引修改 __mu3input_config.json__ 中的值，如果没有这个文件，运行 __Test.exe__ 会自动生成。

### 使用Mageki连接到ongeki-io
1. 点击左上角的设置图标 打开设置页面。
2. 在 __连接__ → __协议__ 中根据 __mu3input_config.json__ 中启用的协议选择：

</br>

　　Udp： UDP

</br>

　　Tcp、Usbmux： TCP

__无线连接在这里即可完成，但由于某些未知的原因，不是所有人都能够成功无线连接，如果你没有连接成功，请使用Tcp或Usbmux协议进行有线连接。__

__如需使用有线连接需要额外进行下面操作：__

- 使用USB线缆连接移动设备与PC。
- 如果你使用iOS设备，需要在PC中安装[__爱思助手__](https://www.i4.cn/)，在爱思助手中成功连接设备并显示设备信息后即可关闭。
- 如果你使用Android设备，需要打开 __USB调试__ 并使用[__ADB__](https://developer.android.com/studio/releases/platform-tools)命令进行 __端口转发__ 。
```
./adb forward tcp:4354 tcp:4354
```


<br/>

启动游戏或Test成功连接到IO后， __连接__ → __状态__ 图标显示为 __已连接__。

<br/>

## 操作说明
- Android打开程序，iPhone长按设置按钮后可扫描Aime卡片
- Android，iPad长按设置按钮，iPhone取消扫描卡片后可模拟刷卡。
- 点击按钮或其下方区域均可以触发按钮。
- 设置中隐藏按钮后可方便作为单独的摇杆，读卡器，或侧键使用

<br/>

## 可能遇到的问题

__解答以下问题的前提是在配置Mageki之前能够正常运行，并且解决方案也不一定有效__
### 解压MU3Input后无法正常游戏
可以尝试解压 __configs.zip__ 中的内容至 __package__ 文件夹
### 进入游戏后摇杆没有反应或产生偏移
在游戏中点击 __设置__ 按钮打开设置菜单，点击 __Test键__ ，退出Mageki的设置页面。根据游戏Test页面的提示，进入 __レバー設定__，分别向左右滑动摇杆，使Test页面中十六进制数不再变化。最后保存并退出Test页面。
### 无线连不上
不用试了，改用有线连接
