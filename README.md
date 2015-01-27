
# Quick Start to Liquid SDK for Windows

This document is just a quick start introduction to Liquid SDK for Windows (Windows Phone 8 and Windows Store). You can read the full documentation at [https://onliquid.com/documentation/windows/](https://onliquid.com/documentation/windows/).

To integrate Liquid in your app, just follow the 4 simple steps below.

## Install Liquid in your project

To install the Liquid SDK via NuGet  , run the following command through the Package Manager Console in each of the projects you want to install it to:

```cs
  PM> Install-Package Liquid -Pre
```

## Start using Liquid

#### Add Capability to Application Manifest

Open the visual editor of your Application Manifest, go to Capabilities tab, and check:

* `Internet (Client & Server)` for a Windows Phone project.

* `Internet (Client)` for a Windows Store project.

### 1. Initialize Liquid singleton

In your **App.xaml.cs** file initialize Liquid in `OnLaunched` method:

```cs
using LiquidWIndowsSDK;

protected async override void OnLaunched(LaunchActivatedEventArgs e)  {
    await Liquid.Initialize("YOUR-APP-TOKEN", this, e); 
    /* (...) */ 
}
```

### 2. Identify a user (optional)

If all your users are anonymous, you can skip this step. If not, you need to identify them and define their profile.
Typically this is done at the same time your user logs in your app (or you perform an auto login), as seen in the example below:

```cs
var attrs = new Dictionary<String,Object>
{
  { "age", 23 },
  { "name", "Bob" }
};
await Liquid.Instance.IdentifyUser("USER_ID", attrs);
```

The **username** or **email** are some of the typical user identifiers used by apps.

### 3. Track events

You can track any type of event in your app, using one of the following methods:

```cs
Liquid.Instance.Track("Click Profile Page");
```
or:

```cs
var attrs = new Dictionary<String,Object>
{
  { "Profile ID", 123 }
};
Liquid.Instance.Track("Click Profile Page", attrs);
```

### 4. Personalize your app (with dynamic variables)

You can transform any old-fashioned static variable into a "Liquid" dynamic variable just by replacing it with a Liquid method. You can use a dynamic variable like this:

```cs
await Liquid.Instance.GetStringVariable("welcomeText", "Welcome to our App");
```

### Full documentation

We recommend you to read the full documentation at [https://onliquid.com/documentation/windows/](https://onliquid.com/documentation/windows/).


# Author

Liquid Data Intelligence, S.A.

# License

Liquid is available under the Apache license. See the LICENSE file for more info.

