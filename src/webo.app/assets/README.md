# Installer artwork (Inno Setup wizard)

Put the images here. File names must follow the pattern, so the installer script finds them:

| Image | Where it appears | Files (24-bit BMP) |
|---|---|---|
| Large | Welcome and Finish pages, left side | `wizard-large-164x314.bmp` (100%), `192x386`, `246x459`, `273x556`, `328x604`, `355x700`, `410x797` |
| Small | Top-right corner of the other pages | `wizard-small-58x58.bmp` (100%), `77x77`, `97x97`, `116x116`, `124x124`, `143x143`, `159x159` |

Minimum useful set: large 164x314 + 246x459 + 328x604 + 410x797, small 58x58 + 97x97 + 124x124 + 159x159.
Sizes are from the Inno Setup documentation (WizardImageFile / WizardSmallImageFile). The icons in
`src/Webo.App/Assets/Brand` are not modified: place them on the artwork as they are.
