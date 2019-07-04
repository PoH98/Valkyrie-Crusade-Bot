# 神女控挂机
___
Bug汇报：QQ群号：809233738

> 自定义脚本以及自定义模拟器支持都可以使用C#编写，此软件由学生制作因此充满bug

# [挂机下载](https://github.com/PoH98/Valkyrie-Crusade-Bot/releases)
___
* 可使用EmulatorController进行一系列的模拟器操作，如果需要自行写别的游戏脚本，直接导入ImageProcessor.dll即可
* 挂机自定义都是使用C# interface插件dll编写，因此方便修改以及增加。
  * 自定义模拟器: EmulatorInterface (导入BotFramework.dll)
  * 自定义脚本：BattleScript（导入VCBot.exe）

# 自定义模拟器
___
* 加载了EmulatorInterface后，必须注意的事项：
  * CloseEmulator() 为关闭模拟器时需要干什么进行关闭，可使用Variables.Proc进行强制关闭(不建议)
  * ConnectEmulator() 把模拟器的IntPtr以及程序加载进入EmulatorController.handle以及Variables.Proc为主的功能，根据每个模拟器不一样都需要不一样的方式进行加载
  * EmulatorName() 模拟器名字，可以用于显示在挂机内
  * LoadEmulatorSettings() 载入模拟器的安装路径，Adb端口，共享文件夹（安卓内:Variables.AndroidSharedPath以及电脑内Variables.SharedPath的路径都需要）
  * SetResolution(int x, int y) 设置模拟器分辨率，DPI等等资料专用
  * StartEmulator() 启动模拟器的方式，必须想办法支持启动可多开的模拟器，否则挂机会出错！

# 自定义脚本
___
* 加载了BattleScript后，必须注意的事项：
  * ReadConfig() 加载脚本所需设置，推荐使用Variables.Configure进行储存和获取资料，无需再创建大量储存资料的垃圾文件。
  * CreateUI() 创建UI，并且显示在挂机主程序内，可以增加功能给
  * ScriptName() 脚本名字
  * Attack() 发动技能的方式，例如可一个个点或者默认的找图长按发动卡牌技能，看个人喜好
  
___  
___
# Bot Framework Documentation
 * Here is some [documentation](https://github.com/PoH98/Valkyrie-Crusade-Bot/wiki) on how to use the BotFramework.dll to make your new script for any games!
 * Warning, I am not responsible for using 'Bots' or 'Scripts' which cause your game account is banned! Use it with more randoms and longer delays will reduce the ban risk!
