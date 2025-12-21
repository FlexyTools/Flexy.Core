![Img](Src/Cover.webp)

[Flexy.Tools](https://github.com/FlexyTools/Flexy.Docs/tree/main) / [Framework](https://github.com/FlexyTools/Flexy.Docs/tree/main/Framework) / Flexy.Core


# Flexy.Core

Core package of **Flexy.Framework** that every other package depends on  
It is Glue fo Flexy.Framework and based on [Flexy.Briks\ToYs](https://github.com/FlexyTools/Flexy.Docs/blob/main/Flexy.Bricks-ToYs) architecture 

[Scripting Api](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/ScriptingApi/Readme.md)
<!--| [Unity Forum](https://discussions.unity.com/t/flexy-gamesettings-free-easily-store-game-settings-with-just-one-line-per-setting/1700923)
| [AssetStore](https://u3d.as/3LKx)  
| [Showcase(Template project)](../../GameTemplates/Barley-Breaks/Readme.md) 
-->


## Content

### [GameContext](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/GameContexts.md)
Flexy way to think about game dependencies and their composition. Context based
You can think like DI Container but more clear and tied to scenes and GameObjects so you can get GameContext from every GO or scene

### [Binders](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/Binders.md)
Core of Flexy.Binding system  
This is one of basis parts of **Flexy.Bricks** **MC-VMV** pattern for binding View to ViewModel   
It is here because it can be used not only in UI but in coregame too

### [Actions](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/Actions.md)
Universal and composable system to do action in response to event:  
play sfx, show vfx, play animation, enable object, change color... actually any action.

### [Common Utilities](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/CommonUtilities.md)
Small set of very often and common used utilities


## Key Strengths

- It is Free
- Customisable:
    - Mostly consists of small types that dont need customization
    - GameContext is highly customizable and even can be connected to another container
- Modular: here only base glue layer is provided all other parts implemented as extensions in other packages


## Technical details

- UniTask based async init
- Native C# Nullability annotations
- C# 10
- Fast Enter Play Mode support

<br/>

[Flexy.Tools](https://github.com/FlexyTools/Flexy.Docs/tree/main) / [Framework](https://github.com/FlexyTools/Flexy.Docs/tree/main/Framework) / Flexy.Core