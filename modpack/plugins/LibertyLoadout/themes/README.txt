Liberty Loadout - style keys
============================

This file lists every look key. Behaviour (buttons, hold time, what is
switched on) stays in LibertyLoadout.ini and is never read from a theme.

How Theme works
---------------
Theme in LibertyLoadout.ini names a file in this folder, without .ini.

  Theme Liberty     shipped default. Edit Liberty.ini to change the look
                    you see in-game.
  Theme GTAV        the GTA V-style file.
  Theme (blank)     style keys are read from LibertyLoadout.ini instead.
                    That is how every version through 0.8.0 worked, and
                    it is what keeps an upgraded install drawing keys you
                    pasted in the main file.

A named theme owns the whole look. Keys it leaves out fall back to the
shipped defaults, not to LibertyLoadout.ini. Colour, size and shape keys
in the main INI are ignored while a Theme is set.

A missing or misspelled name leaves the default look and says so in
GTAIV\LibertyLoadout.log.

Copy a theme under a new name and point Theme at the copy if you want
your edits to survive an update.

First knobs
-----------
  WheelSegmentGap       how far apart the wedges sit; the ring's whole feel
  WedgeCornerRadius     how hard the corners of a wedge are cut
  WedgeOpacity          how much of the scene comes through the ring
  TintAlpha             how dark the rest of the screen goes (0-255)
  WeaponAccentRed/Green/Blue    selection colour on the weapon wheel
  WeaponIcons           HD, Color, Original, or OriginalUpscaled

Defaults below are Liberty's. GTAV.ini lists what it disagrees with, plus
every key whose default moved when Liberty was modernised - a theme that
omits a key follows the default, so GTA V's square, gapless, untracked ring
has to say so rather than inherit.


Ring
----
WheelScale                  0.50-1.75     default 1.00
  Scale either ring when the per-wheel keys below are absent.

WeaponWheelScale            0.50-1.75     default 1.00
  Scale the weapon ring. Falls back to WheelScale.

RadioWheelScale             0.50-1.90     default 1.00
  Scale the radio ring. Falls back to WheelScale. Extra headroom for
  long station lists.

CenterTextScale             0.50-2.00     default 1.50
  Scale the text inside either hub.

HubTextFollowsWheelScale    TRUE / FALSE  default FALSE
  Grow hub glyphs with the ring, not just their line spacing.

WeaponUnarmedPosition       Top / Bottom  default Top
  Where the fist sits. Bottom puts it at six o'clock.

RadioOffPosition            Top / Bottom  default Top
  Where the radio OFF wedge sits.

WedgeRed/Green/Blue         0-255         default 24, 28, 34
  Wedge fill. Also drives inner/empty fills and the dark glass under
  the cursor.

WedgeOpacity                0.00-1.00     default 0.62
  How opaque those fills are. Lower values let the scene through.

WedgeGradient               0.00-1.00     default 1.00
  How much lighter a wedge is at its inner edge. 0 is one flat colour.

RimRed/Green/Blue           0-255         default 208, 218, 230
  Rim hairlines, and the fallback colour for seams and wedge outlines.

RimAlpha                    0-255         default 0
  Alpha of the two rim hairlines. 0 drops them. Liberty runs without them:
  the ring is separate tiles with a gap between, so a rim tying them back
  together works against that. OuterTrack below frames the ring instead.

SeamRed/Green/Blue          0-255         default follows Rim*
  Colour of the seams between wedges.

SeamAlpha                   0-255         default 0
  Alpha of those seams. 0 drops them. Scene shows in the gap only
  while WheelShadowAlpha is 0. Seams and a wide WheelSegmentGap say the
  same thing twice, so a ring with real gaps wants this at 0.

WedgeOutlineRed/Green/Blue  0-255         default follows Rim*
  Hairline around every wedge, not just the selected one.

WedgeOutlineOuterAlpha      0-255         default 0
WedgeOutlineInnerAlpha      0-255         default 0
WedgeOutlineSideAlpha       0-255         default 0
  One alpha per edge. All 0 leaves the ring with rim and seams only.
  Outer and inner fills stop short of those bands, so the alpha is
  transparency against the scene, not a glaze over the tile.

WedgeOutlineWidth           0.00-20.00    default 1.00
  Thickness of those edges, in design-space pixels.

WedgeOutlineUsesAccent      TRUE / FALSE  default FALSE
  Draw the selected wedge's outline in the accent colour.

WheelOuterRadius            40-400        default 182
  Outer radius of the ring, 1280x720 design space.

WheelInnerRadius            0 to outer-20 default 104
  Hub radius. Held at least 20 under the outer radius.

WheelSegmentGap             0.00-5.00     default 3.20
  Half the gap between wedges, in degrees. 0 butts them together. This is
  the shared fallback for the two per-wheel keys below.

WeaponWheelSegmentGap       0.00-5.00     default follows WheelSegmentGap
RadioWheelSegmentGap        0.00-5.00     default follows WheelSegmentGap
  The gap each ring actually draws. They want different values because the
  rings are not the same shape. The weapon ring is always nine wedges at a
  40-degree pitch, where a wide gap is what makes the tiles read as separate
  objects. A radio page can hold RadioStationsPerPage stations plus OFF -
  up to thirty-three wedges at an 11-degree pitch - where the same gap would
  leave slivers. Liberty uses 3.20 and 1.60.

  Whatever either says, the gap is also capped at a fifth of one wedge's
  pitch, so a long station list degrades gracefully instead of drawing a
  ring of hairlines.

WedgeCornerRadius           0-24          default 7
  Radius of the cut taken out of each of a wedge's four corners, in
  design-space pixels. 0 leaves the sharp arc-sector corners a ring drawn
  as one continuous donut wants.

  Clamped twice as it is resolved: to half the ring's radial width, so the
  corners cannot meet and invert the tile, and to a share of the wedge's
  angular width at the inner radius, so a narrow radio wedge pinches
  gracefully rather than closing up. A theme asking for 24 on a
  thirty-three wedge page gets about 18 and keeps a readable tile.

WedgeCornerStyle            Round / Chamfer   default Round
  Round is a quarter-circle fillet; Chamfer is a straight cut across the
  same span. Nothing the wheel draws is antialiased, so a curve pays for
  its smoothness in chord density while a straight cut is exact at any
  size: on a small ring, or a display where Round reads stepped, Chamfer
  is the one that always looks deliberate.

OuterTrackWidth             0-12          default 0
OuterTrackAlpha             0-255         default 46
OuterTrackGap               0-24          default 7
  A faint continuous track outside the rim, in the Rim colour, with the
  wheel's accent lighting only the arc over the wedge under the cursor.
  Width 0 is off, which is how Liberty ships.

  It was meant to be the piece of chrome that held the ring together once
  the wedges were pulled apart into tiles, and on paper that is a good idea.
  In practice, at any weight heavy enough to see, it reads as a second and
  unrelated ring sitting around the wheel rather than as part of it - the
  tiles hold together perfectly well on their own. It is kept because a
  theme with a tighter gap, or one that wants a progress-ring look, may get
  more out of it than the shipped ring does.

  The gap is measured from the rim's outer face, so 0 sits flush outside
  it. The track is drawn UNDER the tiles, for the reason the rim is - the
  highlighted wedge lifts past the outer radius - so a theme that wants the
  track visible beside a lifted selection needs a gap wider than
  HighlightLiftOuter plus HighlightStripeWidth. A theme that keeps the
  wheel shadow also wants the gap inside WheelShadowSpread, or the hairline
  lands on raw scene where its alpha will not carry.

WheelHairline               0.50-6.00     default 1.00
  Thickness of rim and seam hairlines. Never under one pixel.

WeaponIconSize              4-120         default 24
  Screen budget weapon silhouettes size to. They may overflow the wedge,
  and a ring with wide gaps makes that overflow more visible, which is why
  this is a little under the 26 a gapless ring could carry.

WeaponIcons                 HD / Color / Original / OriginalUpscaled
  code fallback HD; Liberty theme ships OriginalUpscaled; GTAV ships Color
  HD is RollY's greyscale redraw. Color is stonem09's painted set and
  skips IconRed/Green/Blue so the paint stays as drawn. Original is
  GTA IV's HUD extracts. OriginalUpscaled is those extracts after a
  local upscale. Pool cues keep the extracts on every setting.

RadioIconSize               4-120         default 22
  Upper bound for station logos.

ParachuteBadgeRadius        8-80          default 25
  Radius of the parachute shortcut circle, in design-space pixels.

ParachuteBadgeGap           -120 to 400   default 14
  Space between the weapon-wheel rim and the shortcut circle. Negative
  values overlap the ring. Liberty used 30 while it drew a wheel shadow the
  badge had to clear; with that shadow off, 14 sits it back against the
  ring. A theme that turns the shadow on wants the larger figure back.

ParachuteBadgeAngle         0-360         default 48
  Position around the weapon wheel in degrees: 0 is right, 90 is below,
  180 is left, and 270 is above. GTAV.ini uses 52.

ParachuteIconScale          0.25-2.50     default 1.25
  Scale the parachute art inside its circle.

ParachuteIconOffsetY        -80 to 80     default -4.5
  Move the parachute art vertically in design-space pixels. Negative is up.

ParachuteLabelScale         0.50-2.00     default 1.00
  Scale the keyboard or controller prompt inside the circle.

ParachuteLabelOffsetY       -80 to 80     default 8.5
  Move the prompt vertically in design-space pixels. Negative is up.

  Badge radius, gap, and icon and label offsets use 1280x720 design space and
  follow WeaponWheelScale. The angle stays fixed. Scaling the wheel therefore
  preserves the badge's position relative to the ring, but moves the whole
  assembly toward the screen edge. At large scales or narrow aspect ratios,
  reduce ParachuteBadgeGap or ParachuteBadgeRadius, or change
  ParachuteBadgeAngle. Gap is measured from the ring, not its shadow; a theme
  with WheelShadowSpread should leave enough extra gap to clear that shadow.

HighlightEquippedParachute  TRUE / FALSE  default TRUE
  Use the weapon-wheel selection fill and episode accent on the badge while
  the parachute is equipped. FALSE keeps the unselected wedge fill.

ParachuteHighlightOutlineWidth  0-20       default 0
  Width of an accent ring around the equipped badge, in design-space pixels.
  This is separate from HighlightStripeWidth, which only affects wedges.

ParachuteBadgeRimAlpha      0-255         default 52
ParachuteBadgeRimWidth      0-20          default 1.00
  Neutral hairline around the badge, in the Rim colour, whether or not the
  parachute is equipped. Alpha 0 is off, which is what a ring whose wedges
  carry no edge of their own wants; otherwise the badge is the one shape on
  screen without one.

WheelShadowAlpha            0-255         default 0
  Dark disc under the ring. 0 removes it.

  A ring cut into separated tiles needs it removed. The disc is solid out to
  the outer radius, so it fills every gap from behind and the tiles stop
  reading as separate objects. HubShadowAlpha does the legibility job it
  used to do for the hub text, confined to the hub.

WheelShadowSpread           0-60          default 16
  How far that disc's falloff reaches past the ring.

HubShadowAlpha              0-255         default 130
  Darkening confined to the hub. 0 is off.

HubShadowSpread             0-400         default 34
  How far back from the inner radius that falloff starts. Ending it inside
  the hub leaves a band of untouched scene between the hub and the tiles,
  which is what makes the hub read as its own object rather than as the
  hole in a donut.


Selection
---------
WeaponAccentRed/Green/Blue              default 240, 186, 56
  Accent on the weapon wedge under the cursor (GTA IV). 0-255.

WeaponAccentTladRed/Green/Blue          default 200, 52, 44
  Same accent on The Lost and Damned.

WeaponAccentTbogtRed/Green/Blue         default 229, 57, 155
  Same accent on The Ballad of Gay Tony.

EpisodicWeaponAccent        TRUE / FALSE  default TRUE
  FALSE uses WeaponAccentRed/Green/Blue on every episode.

RadioAccentRed/Green/Blue               default 86, 200, 224
  Accent on the station wedge under the cursor (GTA IV cyan).

RadioAccentTladRed/Green/Blue           default 200, 52, 44
RadioAccentTbogtRed/Green/Blue          default 229, 57, 155
EpisodicRadioAccent         TRUE / FALSE  default TRUE
  FALSE uses RadioAccentRed/Green/Blue on every episode.

HighlightAccentMix          0.00-1.00     default 0.08
  How much accent is mixed into the highlighted wedge at its outer edge.
  1.00 makes the tile the accent.

  Liberty keeps this low on purpose. The selection leads with its edge -
  the outline, the stripe and the lit arc of the outer track - rather than
  with its fill, which is how a modern wheel marks the wedge under the
  cursor and what keeps a saturated accent from reading as a painted slab.
  GTAV takes the other road and sets it to 1.00.

HighlightAccentMixInner     0.00-1.00     default 0.03
  The same at the inner edge.

HighlightOpacity            0.00-1.00     default 1.00
HighlightOpacityInner       0.00-1.00     default 1.00
  Opacity of the highlighted wedge at each edge, as multipliers on the
  fill already set by WedgeOpacity. Equal values give a flat tile.
  Inner below outer fades toward the hub.

HighlightLiftInner          0-60          default 2
HighlightLiftOuter          0-60          default 4
  How far the highlighted wedge lifts past the ring. 0 is flush. Tiles that
  already float clear of each other need less of this: a wedge that both
  stands apart from its neighbours and lifts out of the ring is saying the
  same thing twice.

HighlightOutlineAlpha       0-255         default 215
  Accent hairline on the other three sides of a lifted plate. 0 for a
  flush highlight.

HighlightOutlineWidth       0.00-20.00    default 1.60
  Thickness of that hairline, in design-space pixels. 0 follows
  WheelHairline, which is what it was fixed to before this key existed.

HighlightStripeWidth        0-60          default 5
  Accent band riding the outer edge of the highlighted wedge, drawn inside
  the tile so it follows the corner cut and tapers with it. 0 drops it.

  Inside rather than in a band beyond the outer radius, which is where it
  sat while the wedges were square. On a cut corner the outside version is
  clamped to the width of the tile's flat top, so it comes out shorter than
  the wedge, ends square while the wedge ends round, and floats a hairline
  clear of the thing it is marking. Riding the edge from within, it reads as
  that edge painted accent - which is what RDR2 does with the arc on the
  wedge under its cursor.

WeaponHighlightGlowAlpha    0-255         default 55
  Accent bloom inside the highlighted weapon wedge. 0 is none. This is the
  part of the highlight that behaves as a fill, so an edge-led selection
  keeps it low and lets the outline and stripe carry the signal.

RadioHighlightGlowAlpha     0-255         default 65
  Same bloom on the radio wheel.


Equipped
--------
CurrentMarkRed/Green/Blue   0-255         default 206, 216, 228
  Colour of the bar marking the equipped weapon or tuned station.

CurrentMarkWidth            0-60          default 2
  Thickness of that bar. 0 drops the bar and the wash under it.

CurrentGlowAlpha            0-255         default 40
  Wash off the marked edge. 0 leaves the bar alone.

CurrentMarkPosition         Inner / Outer default Inner
  Which edge the bar rides.

CurrentMarkUsesAccent       TRUE / FALSE  default FALSE
  Colour the bar with the wheel's accent instead of CurrentMark*.

CurrentMarkOnHighlight      TRUE / FALSE  default FALSE
  Keep the mark on the equipped wedge while the cursor is on it.


Icons, hub and panel
--------------------
IconRed/Green/Blue          0-255         default 238, 242, 246
  Weapon silhouettes, station logos, and the hub cursor pointer.
  Color icons skip this tint.

PointerUsesAccent           TRUE / FALSE  default TRUE
  Colour the hub pointer with the accent instead of Icon*. On a wheel whose
  selection is carried by its edges rather than its fill, the pointer is
  part of that language and wants the same colour.

ShowHubPointer              TRUE / FALSE  default TRUE
  Triangle in the hub pointing at the selected wedge.

GlyphRed/Green/Blue         0-255         default follows Icon*
  Stand-in icon for a missing station logo, and the line-drawn OFF power
  glyph if the embedded OFF badge failed to load. The shipped OFF badge
  follows Icon* like the station logos.

GlyphOutlineRed/Green/Blue  0-255         default 0, 0, 0
GlyphOutlineAlpha           0-255         default 0
  Dark border under those line-drawn glyphs. 0 is off. The shipped OFF
  badge already has an outline in the art.

GlyphOutlineWidth           0.50-6.00     default 1.00
  How far that border reaches past the glyph.

HubTextRed/Green/Blue       0-255         default 255, 255, 255
  Hub title colour. Dimmer ranks (detail, tracks, eyebrow, ammo) scale
  from this.

ShowHubEyebrow              TRUE / FALSE  default TRUE
  Category line above the name in the hub.

HubEyebrowTracking          0.00-4.00     default 2.20
  Extra space between the letters of that line, in design-space pixels per
  gap. 0 is the font's own spacing.

  CFont cannot track a string, so a non-zero value costs one print per
  glyph. An eyebrow is one short word, so that is a handful of calls - but
  it is why there is no equivalent key for a rank that can carry a whole
  station name.

HubRuleAlpha                0-255         default 54
HubRuleWidth                0.00-1.00     default 0.44
  Hairline rule between the eyebrow and the name, in the Rim colour, with
  its length given as a fraction of the hub's width. Alpha 0 is off.

  It is what gives a hub holding two ranks of text something to hold onto
  once the ring around it has been pulled apart into separate tiles: the
  composition needs a centre, and two lines floating in a disc is not one.

HubTextEdge                 0.00-1.50     default 0.60
  Outline weight on hub text. 0 removes it. On-wedge ammo keeps its
  own outline.

StyleShortcutTextScale      0.50-2.00     default 1.00
  Scale the theme/icon shortcut hints and change confirmations shown at the
  top centre of the screen.

DimAmmoClip                 TRUE / FALSE  default TRUE
  Dim the "/ 30" clip half of on-wedge ammo counts.

AmmoClipScale               0.50-1.00     default 0.78
  Scale that clip half relative to the reserve. 1.00 draws both at one
  size, which is what DimAmmoClip alone has always done. Below that the
  count becomes a large value with a small denominator hung off it, which
  is what makes the number worth reading - how much you have - legible from
  further away than the magazine size it is divided by. Only applies while
  DimAmmoClip is on.

PanelRed/Green/Blue         0-255         default follows Wedge*
  Weapon stat card colour.

PanelOpacity                0.00-1.00     default 0.62
  How opaque the stat card is.

PanelRimAlpha               0-255         default 0
  Alpha of the card's rim hairline. 0 unboxes the card and leaves the
  accent rule along its top as the only framing, which is how a modern
  panel is set. Raise it toward 30 if the card loses its edge against a
  near-black interior; the shipped 0 leans on the background blur to give
  it something to sit against.

WeaponStatsPosition         TopRight / TopLeft / BottomLeft / BottomRight
  default TopRight
  Screen corner for the stat card.

WeaponStatsAccentRule       TRUE / FALSE  default TRUE
  Accent line along the top of the card.

WeaponStatsLayout           Inline / Stacked  default Inline
  Label beside the bar, or above a full-width bar.

WeaponStatsLabelAlign       Left / Right  default Right
  Which way an inline label sits in its column. Right pushes the labels
  against the bars, so four words of different lengths read as one tidy
  edge rather than a ragged column. Stacked rows ignore this: their label
  is above a full-width bar and has nothing to align against.

  The column itself is measured from the labels rather than fixed, so the
  longest one starts at the card's own inset and the rest of the width goes
  to the bars. That matters most with Right: a fixed column wide enough for
  the longest word leaves the difference as dead space, and right-aligning
  collects all of it against one edge of the card, where it reads as a
  margin nobody asked for. Heavily tracked or translated labels are capped
  at 55% of the card so they cannot crowd the bars out.

WeaponStatsRowHeight        10-80         default 24
  Vertical pitch of one stat row.

WeaponStatsLabelGap         0-40          default 3
  Gap between a stacked label and the bar under it.

WeaponStatsWidth            140-420       default 268
  Width of the stat card.

WeaponStatsBarHeight        2-24          default 6
  Thickness of one stat bar.

StatBarSegments             0-24          default 12
StatBarSegmentGap           0-8           default 2
  Break each bar into this many discrete ticks instead of one continuous
  fill, with that gap between them in design-space pixels. 0 segments is
  the continuous bar.

  A ticked bar makes the difference between two weapons countable rather
  than approximate, which is the point of a comparison panel, and its fill
  ends on a tick boundary instead of mid-pixel. The gain and loss tips keep
  working: each tick takes its colour from where its own centre falls
  against the same thresholds the continuous bar uses.

StatLabelTracking           0.00-4.00     default 1.60
  Extra space between the letters of the row labels, in design-space pixels
  per gap. Same mechanism and the same caution as HubEyebrowTracking.

WeaponStatsPadX             0-60          default 14
WeaponStatsPadY             0-60          default 12
  Inset from the card's edges to its content.

WeaponStatsTitleCase        TRUE / FALSE  default FALSE
  "Fire Rate" instead of "FIRE RATE".

StatBarRed/Green/Blue       0-255         default follows Icon*
  Filled part of a stat bar.

StatTrackRed/Green/Blue     0-255         default follows Panel*
  Unfilled part of a stat bar.

StatLabelUsesBarColour      TRUE / FALSE  default FALSE
  Print row labels in the bar colour.

StatGainRed/Green/Blue      0-255         default 72, 200, 110
  Gain tip on a weapon stat bar.

StatLossRed/Green/Blue      0-255         default 220, 64, 64
  Loss tip on a weapon stat bar.


Background
----------
BackgroundBlur              TRUE / FALSE  default TRUE
  Blur the scene behind an open wheel.

BackgroundBlurPasses        1-4           default 3
  How many times the copy is halved.

TintRed/Green/Blue          0-255         default 14, 12, 11
  Full-screen wash colour over the blur.

TintAlpha                   0-255         default 92
  Wash opacity. BackgroundBlur FALSE plus TintAlpha 130 is the 0.7.0
  background. A ring with translucent tiles wants a little more of this
  than an opaque one did, because more of the scene now reaches the eye
  through the wheel itself.

BackgroundVignette          TRUE / FALSE  default TRUE
  Darken the screen edges under the wash.

BackgroundVignetteStrength  0-255         default 90
  Vignette alpha in the corners.

BackgroundVignetteSpread    0.20-0.95     default 0.55
  Where the vignette starts, as a fraction of centre-to-corner.
  Lower is wider.
