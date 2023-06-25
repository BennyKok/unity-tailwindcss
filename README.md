# Tailwind CSS for Unity UIToolkit

⚠️ This is a proof of concept and not ready for production use.

The core idea to use the same Tailwind CSS syntax to style Unity UIToolkit UI elements.

---

https://cdn.discordapp.com/attachments/750213962716545054/1122496614825738251/unity-tailwind.mp4

## Roadmap

- [x] Basic styling (w-full, text-white, bg-red-500, etc)
- [x] Basic layout (flex, flex-col, flex-row, etc)
- [ ] Support Transform
- [ ] Support Transition
- [ ] Support Hover State
- [ ] Update color palette to latest Tailwind CSS
- [ ] Custom Built-in UI elements that enable styling 

## Install

Via UPM.

```
UPM install via git url -> https://github.com/BennyKok/unity-tailwindcss.git
```

You can also choose to add this as a submodule in your package folder.

```
git submodule add https://github.com/BennyKok/unity-tailwindcss.git Packages/unity-tailwindcss
```

## Note

Current implementation is by generating uss compatible utility css class that follows the Tailwind CSS syntax. A better way would be implementing a JIT compiler that generates the uss file on the fly. However that would enable more work to be done. Currently this system if a POC that we can use to evaluate the feasibility of this project. Whether this is a better system and improve UITK usability.

## Explore
More about BennyKok and his Unity related work.

[Twitter](https://twitter.com/BennyKokMusic) | [Website](https://bennykok.com) | [AssetStore](https://assetstore.unity.com/publishers/28510)