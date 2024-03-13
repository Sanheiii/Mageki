# 使用手册
Mageki可以使用移动设备来连接到[ongeki-io](https://github.com/Sanheiii/ongeki-io)，来到这个页面的你一定已经迫不及待了，但是首先你需要一个能够成功运行并使用其它方式控制的游戏本体。如果还没有，请参阅：
### [最新最热的游戏下载与配置说明](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app)
当游戏本体配置完备后，才可以进行下面的步骤。

### 注意事项：
- 适配
  - 本教程仅适用于 [Mageki]([https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app](https://github.com/Sanheiii/Mageki)) 的配置教程，请勿拿别的手台使用Mageki教程并配置，Its Useless。
- 教程中的 [蓝字](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app)
  - 教程中的所有 [蓝字](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app) 都是可以 [点击](https://sddt-dist.sys-all.xyz/135/app/SDDT_1.35.01_20220401114514_0.app) 的，如果你Stuck在哪个步骤了，不妨按下这个神奇的蓝色按钮呢？
- 必要性
  - 本教程是Mageki非常必要的一个Getting Start篇章，如果您在初次配置完后发现连接不上，请按照可能遇到的问题开始排查。

# 开始使用
请前往 __[发布页](https://github.com/Sanheiii/Mageki/releases)__ 下载所需的文件。
### 安装程序
- Android
  1. 在 [发布页](https://github.com/Sanheiii/Mageki/releases) 下载 __mageki_x.x.x.apk__。
  2. 在Android设备上打开以安装此程序。
- iOS
  - 进入 [发布页](https://github.com/Sanheiii/Mageki/releases) 按照提示操作。
- UWP
  - 缺少触屏设备无法调试，如有需求请自行编译，Debug并提交Pull request，在PR成功前下文不提供技术支持。
### 配置ongeki-io
1. 将你在[发布页](https://github.com/Sanheiii/Mageki/releases)上下载的__MU3Input_x.x.x.zip__ 的内容解压至 __SDDT\package__ 目录下。
2. 修改在 __segatools.ini__ 中 __[mu3io]__ 与 __[aimeio]__ 下的内容，如果没有，则添加它们。
``` ini
[mu3io]
path = MU3Input.dll

[aimeio]
path = MU3Input.dll
```
3. 根据 [__mu3input_config.json说明__](https://github.com/Sanheiii/Mageki/wiki/mu3input_config.json) 中的指引修改 __mu3input_config.json__ 中的值，如果没有这个文件，运行 __Test.exe__ 会自动生成。
4. 第三步很关键，如果您并未修改此文件使其设置与您设备对应，您将可能回来看这里第二遍。

### 使用Mageki连接到ongeki-io
1. 点击左上角的设置图标 打开设置页面。
2. 在 __连接__ → __协议__ 中根据 __mu3input_config.json__ 中启用的协议选择：


　　Udp： UDP

</br>

　　Tcp、Usbmux： TCP

</br>
###请注意：此处的含义为，当您在mu3input_config.json中使用UDP协议时，在客户端处应选择UDP连接，使用TCP/Usbmux时应选择TCP连接，而不是修改mu3input_config.json的内容</br>

__无线连接在这里即可完成，但由于某些未知的原因，不是所有人都能够成功无线连接，如果你没有连接成功，请使用Tcp或Usbmux协议进行有线连接。__

__如需使用有线连接需要额外进行下面操作：__

- 使用USB线缆连接移动设备与PC。
- 如果你使用iOS设备，需要在PC中安装[__爱思助手__](https://www.i4.cn/)，在爱思助手中成功连接设备并显示设备信息后即可关闭。
- 如果你使用Android设备，需要打开 __USB调试__ 并使用[__ADB__](https://developer.android.com/studio/releases/platform-tools)命令进行 __端口转发__ 。
- 上面的蓝字都可以点击，如果您不想上网搜索，请直接点击上方蓝字到达工具官网，也可以右键在新页面打开，关于如何安装，请自行百度。
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

__解答以下问题的前提是您的游戏在配置Mageki之前能够正常运行，并且解决方案也不一定有效，请坐和放宽__
### 解压MU3Input后无法正常游戏
可以尝试解压 __configs.zip__ 中的内容至 __package__ 文件夹。
### 进入游戏后摇杆没有反应或产生偏移
在游戏中点击 __设置__ 按钮打开设置菜单，点击 __Test键__ ，退出Mageki的设置页面。根据游戏Test页面的提示，进入 __レバー設定__，分别向左右滑动摇杆，使Test页面中十六进制数不再变化。最后保存并退出Test页面。
### 有线连不上
- 安卓
  1. 请检查线缆有无正常链接至安卓设备，并且设备已经开启了开发者模式
  2. 请确认电脑端有显示出你的安卓设备，并且已经安装好了[__ADB Tools__](https://developer.android.com/studio/releases/platform-tools)。
  3. 使用指令 ./adb forward tcp:4354 tcp:4354 转发
  4. 请确认您的设备上正确设定了和[__mu3input_config.json__](https://github.com/Sanheiii/Mageki/wiki/mu3input_config.json)中相对应的值。
  5. 启动游戏/启动Test，您将会看见 连接 → 状态 图标显示为 已连接
<br/>
- ios
  1. 请在电脑上下载爱思助手[__爱思助手__](https://www.i4.cn/)，并且在上面成功显示你的设备。
  2. 在mu3input_config.json内更改Type为Usbmux模式
- 还搞不定怎么办
  1. 请重新阅读上方教程
  2. 详细列出故障现象，已经配置好的内容，以及一个和善的心态去群内问如何解决。
### 无线连不上
  1.请检查你的两台设备是否处在同一局域网。
  2.缘分没到，不用试了，改用有线连接。
