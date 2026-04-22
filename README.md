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
- `TextMeshWobbleScale`: animates each visible character with position wobble and scale wobble.
- `TextMeshFirstCharacterScale`: scales only the first visible character in a `TextMeshProUGUI`.
- `TextMeshPikaBlink`: strips `<Pika>...</Pika>` tags and blinks that range from its original color to white.

## Serialized Fields

- `speed`: animation speed multiplier.
- `shakeRadius`: per-character wobble amplitude.
- `scaleAmplitude`: per-character scale wobble amplitude.
- `scaleMultiplier`: scale applied to the first visible character.
- `sourceText`: raw string that may include `<Pika>...</Pika>` tags.
- `blinkSpeed`: speed of the original-color to white blink.

## Notes

- Requires `TextMeshProUGUI`.
- Uses `UniTask` for the update loop.
