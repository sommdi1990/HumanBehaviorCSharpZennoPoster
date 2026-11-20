# Interaction planning for ZennoPoster

This C# library represents browser interactions as plans containing pointer, scroll and timing decisions. An adapter translates these plans into calls on a ZennoPoster surface.

Start with the interaction surface interface to see what the planner can request. The planner works with model objects, while the ZennoPoster adapter owns integration details. Actual interaction requires a compatible host.

- [IInteractionSurface.cs](HumanBehavior.Core/Abstractions/IInteractionSurface.cs)
- [ActionPlanner.cs](HumanBehavior.Core/Internal/ActionPlanner.cs)
- [ZennoPosterSurface.cs](HumanBehavior.Core/Adapters/ZennoPosterSurface.cs)

## مشخصات پروژه

- **Target Framework**: .NET Framework 4.6.2
- **زبان**: C# 7.3
- **وابستگی‌ها**: Newtonsoft.Json 13.0.3

## تنظیمات (HumanBehaviorOptions)

- **ClickBias**: احتمال کلیک (پیش‌فرض: 0.4)
- **ScrollBias**: احتمال اسکرول (پیش‌فرض: 0.3)
- **HoverBias**: احتمال هاور (پیش‌فرض: 0.2)
- **PauseBias**: احتمال مکث (پیش‌فرض: 0.1)
- **PointerNoiseInPixels**: نویز تصادفی برای موقعیت ماوس (پیش‌فرض: 8)
- **MaxActionsPerRun**: حداکثر تعداد عمل در هر اجرا (پیش‌فرض: 25)
- **MinPauseBetweenActions**: حداقل زمان بین اعمال (پیش‌فرض: 900ms)
- **MaxPauseBetweenActions**: حداکثر زمان بین اعمال (پیش‌فرض: 2400ms)

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
