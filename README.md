# Doto Rich TMP

Reusable TextMeshPro effects package for Unity.

## Install As Submodule

Add this repository as a git submodule somewhere outside `Assets`, for example:

```bash
git submodule add <repo-url> ExternalRepos/Doto-RichTMP
```

Then reference it from `Packages/manifest.json`:

```json
"com.doto.rich-tmp": "file:../ExternalRepos/Doto-RichTMP"
```

## Components

- `TextMeshWobble`: add to any `TextMeshProUGUI` object to animate each visible character.
- `TextMeshFirstCharacterScale`: scales only the first visible character in a `TextMeshProUGUI`.

## Serialized Fields

- `speed`: animation speed multiplier.
- `shakeRadius`: per-character wobble amplitude.
- `scaleMultiplier`: scale applied to the first visible character.

## Notes

- Requires `TextMeshProUGUI`.
- Uses `UniTask` for the update loop.
