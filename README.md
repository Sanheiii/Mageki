# Mageki
[![latest](https://img.shields.io/github/v/release/sanheiii/mageki.svg?style=plastic)](https://github.com/Sanheiii/Mageki/releases/latest)
[![GitHub issues](https://img.shields.io/github/issues/Sanheiii/Mageki?style=plastic)](https://github.com/Sanheiii/Mageki/issues)
[![GitHub forks](https://img.shields.io/github/forks/Sanheiii/Mageki?style=plastic)](https://github.com/Sanheiii/Mageki/network)
[![GitHub stars](https://img.shields.io/github/stars/Sanheiii/Mageki?style=plastic)](https://github.com/Sanheiii/Mageki/stargazers)
[![GitHub license](https://img.shields.io/github/license/Sanheiii/Mageki?style=plastic)](https://github.com/Sanheiii/Mageki/blob/master/LICENSE)

如果你喜欢，可以点击star支持我
## 关于
Mageki能够连接到局域网中的ongeki-io，使你的移动设备成为音击控制器。
## 请注意
- __项目名中的Ma取自Xamarin，geki取自ONGEKI，请不要使用其它任何汉字或Emoji来代替它们。__
- __不提供游戏下载，如有需求请另寻他处获取。__
## 特性
- 能够切换到便于定位的简易布局方便小屏玩家的游玩。
- 为移动设备特别优化的操作方式。
- 在打歌时自动隐藏无用按键。
- 使用设备的NFC功能读取Felica标签登录游戏（仅支持Android设备）。
- 自由地调节按钮高度。
## 开始使用
请前往[发布页](https://github.com/Sanheiii/Mageki/releases)下载所需的文件。
### 安装程序
- Android
  - 下载 __mageki.apk__。
  - 在Android设备上打开以安装程序。
- iOS
  - 下载 __mageki.ipa__。
  - 使用自签工具（如[AltStore](https://altstore.io/)）安装ipa。
- UWP
  - 缺少触屏设备无法调试，如有需求请自行编译，Debug并提交Pull request。
### 配置IO
- 将 __mu3hook.dll__，__mu3Input.dll__ 解压至 __SDDT\package__ 目录下。
- 在原有的 __segatools.ini__ 内添加以下文本
```ini
[mu3io]
path=MU3Input.dll
port=4354
protocol=udp

[aimeio]
path=MU3Input.dll
```
- 保存文件

完成上面操作后，确保运行Mageki与游戏的设备处于同一局域网并没有连接到代理软件即可使用，但仍需一些额外操作来获得更好游戏体验。
### 校准摇杆
- 打开游戏与Mageki，点击Mageki中间的logo进入设置，点击 __游戏测试菜单__ 按钮进入游戏测试菜单来校准摇杆。

### 操作说明
- 点击Logo打开程序设置。
- 长按Logo模拟刷卡。
- 点击按钮与其下方区域均视为点击按钮。
- 在屏幕任意位置水平滑动模拟摇杆。
- 点击空白部分模拟侧键。
- 简易布局下使用两指点击同一按钮可同时触发两侧按键。

## 待完善
- 限定侧键范围，并在屏幕中间一块区域映射摇杆的绝对位置。
- 设置页面的placeholder颜色显示异常
