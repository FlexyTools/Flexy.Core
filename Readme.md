![Img](https://github.com/user-attachments/assets/42049fd9-6985-4e3f-93f2-b3b91fe7f658)
    
[Flexy.Tools](https://github.com/FlexyTools/Flexy.Docs/tree/main) / [Framework](https://github.com/FlexyTools/Flexy.Docs/tree/main/Framework) / Flexy.Core


# Flexy.Core

Core package of **Flexy.Framework** that every other package depends on  
It is Glue fo Flexy.Framework and based on [Flexy.Briks\ToYs](https://github.com/FlexyTools/Flexy.Docs/blob/main/Flexy.Bricks-ToYs) architecture 

[Docs](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.Core/Readme.md)
| [Unity Forum](https://discussions.unity.com/t/a/1701330)
<!--| [AssetStore](https://u3d.as/3LKx)  
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


### Install

Open Unity Package Manager   
Add package from git URL: https://github.com/FlexyTools/Flexy.Core.git


### Issues and Discussions

Please file any issues with documentation or packages in the [Flexy.Docs](https://github.com/FlexyTools/Flexy.Docs/blob/main/Framework/Flexy.GameSettings/Readme.md) repo

### Have Fun :)

<br/>

[Flexy.Tools](https://github.com/FlexyTools/Flexy.Docs/tree/main) / [Framework](https://github.com/FlexyTools/Flexy.Docs/tree/main/Framework) / Flexy.Core