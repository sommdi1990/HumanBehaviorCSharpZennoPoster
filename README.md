# HumanBehavior - شبیه‌ساز رفتار انسانی برای ZennoPoster

یک کتابخانه C# برای شبیه‌سازی رفتارهای تصادفی انسانی (کلیک، هاور، اسکرول، مکث) در ZennoPoster.

## مشخصات پروژه

- **Target Framework**: .NET Framework 4.6.2
- **زبان**: C# 7.3
- **وابستگی‌ها**: Newtonsoft.Json 13.0.3

## ساختار پروژه

```
src/HumanBehavior.Core/
├── Abstractions/
│   └── IInteractionSurface.cs      # رابط برای سطح تعامل
├── Adapters/
│   └── ZennoPosterSurface.cs       # آداپتر ZennoPoster
├── Models/                          # مدل‌های داده
├── Internal/                        # کلاس‌های داخلی
└── HumanBehaviorSimulator.cs        # کلاس اصلی شبیه‌ساز
```

## نحوه بیلد

### در Windows با Visual Studio

1. پروژه را در Visual Studio 2019 یا 2022 باز کنید
2. روی پروژه راست‌کلیک کنید → **Build**
3. DLL در مسیر `bin/Debug/` یا `bin/Release/` ساخته می‌شود

### در Windows با MSBuild (Command Line)

```cmd
msbuild src/HumanBehavior.Core/HumanBehavior.Core.csproj /p:Configuration=Release
```

### در Linux (نیاز به Mono/MSBuild)

```bash
# نصب Mono (اگر نصب نیست)
sudo apt-get install mono-complete msbuild

# بیلد پروژه
msbuild src/HumanBehavior.Core/HumanBehavior.Core.csproj /p:Configuration=Release
```

## استفاده در ZennoPoster

### 1. اضافه کردن DLL به پروژه

1. فایل `HumanBehavior.dll` را در پوشه پروژه ZennoPoster کپی کنید
2. در ProjectMaker، به **Blocks** → **C# Code** بروید
3. DLL را به پروژه اضافه کنید

### 2. کد نمونه

```csharp
using HumanBehavior.Core;
using HumanBehavior.Core.Adapters;
using HumanBehavior.Core.Models;
using System;
using System.Threading.Tasks;

// instance همان آبجکت ZennoPoster است که به بلاک تزریق می‌شود
var surface = new ZennoPosterSurface(instance);

var options = new HumanBehaviorOptions
{
    ClickBias = 0.45,
    ScrollBias = 0.25,
    HoverBias = 0.2,
    PauseBias = 0.1,
    PointerNoiseInPixels = 10,
    MaxActionsPerRun = 20
};

var simulator = new HumanBehaviorSimulator(
    surface, 
    options, 
    msg => project.SendInfoToLog(msg, true)
);

// اجرای 15 عمل تصادفی
await simulator.RunAsync(actionBudget: 15);

// یا اجرا برای مدت زمان مشخص
// await simulator.RunAsync(TimeSpan.FromSeconds(30));
```

## تنظیمات (HumanBehaviorOptions)

- **ClickBias**: احتمال کلیک (پیش‌فرض: 0.4)
- **ScrollBias**: احتمال اسکرول (پیش‌فرض: 0.3)
- **HoverBias**: احتمال هاور (پیش‌فرض: 0.2)
- **PauseBias**: احتمال مکث (پیش‌فرض: 0.1)
- **PointerNoiseInPixels**: نویز تصادفی برای موقعیت ماوس (پیش‌فرض: 8)
- **MaxActionsPerRun**: حداکثر تعداد عمل در هر اجرا (پیش‌فرض: 25)
- **MinPauseBetweenActions**: حداقل زمان بین اعمال (پیش‌فرض: 900ms)
- **MaxPauseBetweenActions**: حداکثر زمان بین اعمال (پیش‌فرض: 2400ms)

## ویژگی‌ها

- ✅ شناسایی خودکار دکمه‌ها و لینک‌های صفحه
- ✅ کلیک‌های تصادفی با نویز طبیعی
- ✅ اسکرول‌های نرم و تصادفی
- ✅ هاور روی عناصر
- ✅ مکث‌های تصادفی بین اعمال
- ✅ پشتیبانی کامل از ZennoPoster API

## نکات مهم

- این پروژه برای .NET Framework 4.6.2 طراحی شده است
- از `dynamic` برای ارتباط با ZennoPoster استفاده می‌کند (بدون نیاز به reference مستقیم)
- در زمان اجرا، ZennoPoster آبجکت `instance` را تزریق می‌کند

## لایسنس

این پروژه برای استفاده در پروژه‌های ZennoPoster طراحی شده است.



### داخل SharedCode:

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Collections;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;
using ZennoLab.Macros;
using Global.ZennoExtensions;
using ZennoLab.Emulation;
using ZennoLab.CommandCenter.TouchEvents;
using ZennoLab.CommandCenter.FullEmulation;
using ZennoLab.InterfacesLibrary.Enums;
using HumanBehavior.Core;
using HumanBehavior.Core.Models;
using HumanBehavior.Core.Adapters;


namespace ZennoLab.OwnCode
{
	/// <summary>
	/// A simple class of the common code
	/// </summary>
	public class CommonCode
	{
		/// <summary>
		/// Lock this object to mark part of code for single thread execution
		/// </summary>
		public static object SyncObject = new object();

		public static void SimulateHumanBehavior(Instance instance, IZennoPosterProjectModel project)
		{
		    try
		    {
		        var surface = new ZennoPosterSurface(instance);
		        var options = new HumanBehaviorOptions
		        {
		            ClickBias = 0.45,
		            ScrollBias = 0.25,
		            HoverBias = 0.2,
		            PauseBias = 0.1,
		            PointerNoiseInPixels = 10
		        };
		
		        var simulator = new HumanBehaviorSimulator(surface, options, msg => project.SendInfoToLog(msg));
		        
		        // اجرای سناریو با بودجه عملیاتی
		        simulator.RunAsync(actionBudget: 15).GetAwaiter().GetResult();
		        
		        // یا استفاده از زمان‌بندی:
		        // simulator.RunAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
		    }
		    catch (Exception ex)
		    {
				project.SendInfoToLog("Error Human Behavior: " + ex.Message);
		    }
		}
		// Insert your code here
	}
}

```
