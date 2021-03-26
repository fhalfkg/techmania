using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpriteSheet
{
    public string filename;
    public int rows;
    public int columns;
    public int firstIndex;
    public int lastIndex;

    [NonSerialized]  // Loaded at runtime
    public Texture2D texture;
    [NonSerialized]
    public List<Sprite> sprites;

    public SpriteSheet()
    {
        rows = 1;
        columns = 1;
        firstIndex = 0;
        lastIndex = 0;
    }

    // Call after loading texture.
    public void GenerateSprites()
    {
        if (texture == null)
        {
            throw new Exception("Texture not yet loaded.");
        }
        sprites = new List<Sprite>();
        int spriteWidth = texture.width / columns;
        int spriteHeight = texture.height / rows;
        for (int i = firstIndex; i <= lastIndex; i++)
        {
            int row = i / columns;
            // Unity thinks (0, 0) is bottom left but we think
            // (0, 0) is top left. So we inverse y here.
            int inverseRow = rows - 1 - row;
            int column = i % columns;
            Sprite s = Sprite.Create(texture,
                new Rect(column * spriteWidth,
                    inverseRow * spriteHeight,
                    spriteWidth,
                    spriteHeight),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit: 100f,
                extrude: 0,
                // The default is Tight, whose performance is
                // atrocious.
                meshType: SpriteMeshType.FullRect);
            sprites.Add(s);
        }
    }

    public Sprite GetSpriteForFloatBeat(float beat)
    {
        beat = beat - Mathf.Floor(beat);
        int index = Mathf.FloorToInt(beat * sprites.Count);
        index = Mathf.Clamp(index, 0, sprites.Count - 1);
        return sprites[index];
    }
}

[Serializable]
public class SpriteSheetForNote : SpriteSheet
{
    public float scale;  // Relative to 1x lane height

    public SpriteSheetForNote() : base()
    {
        scale = 1f;
    }
}

[Serializable]
public class SpriteSheetForVfx : SpriteSheet
{
    public float scale;  // Relative to 1x lane height
    public float speed;  // Relative to 60 fps

    public SpriteSheetForVfx() : base()
    {
        scale = 1f;
        speed = 1f;
    }

    // Returns null if the end of VFX is reached.
    public Sprite GetSpriteForTime(float time, bool loop)
    {
        float fps = 60f * speed;
        int index = Mathf.FloorToInt(time * fps);
        if (loop)
        {
            index = index % sprites.Count;
        }
        if (index < 0 || index >= sprites.Count) return null;
        return sprites[index];
    }
}

[Serializable]
[FormatVersion(NoteSkin.kVersion, typeof(NoteSkin), isLatest: true)]
public class NoteSkinBase : Serializable<NoteSkinBase> {}

[Serializable]
public class NoteSkin : NoteSkinBase
{
    public const string kVersion = "1";

    // Note skin's name is the folder's name.

    public SpriteSheetForNote basic;

    public SpriteSheetForNote chainHead;
    public SpriteSheetForNote chainNode;
    public SpriteSheetForNote chainPath;

    public SpriteSheetForNote dragHead;
    public SpriteSheetForNote dragCurve;

    public SpriteSheetForNote holdHead;
    public SpriteSheetForNote holdTrail;
    public SpriteSheet holdTrailEnd;
    public SpriteSheetForNote holdOngoingTrail;
    public SpriteSheet holdOngoingTrailEnd;

    public SpriteSheetForNote repeatHead;
    public SpriteSheetForNote repeat;
    public SpriteSheetForNote repeatHoldTrail;
    public SpriteSheet repeatHoldTrailEnd;
    public SpriteSheetForNote repeatPath;

    public NoteSkin()
    {
        version = kVersion;
    }

    public List<SpriteSheet> GetReferenceToAllSpriteSheets()
    {
        List<SpriteSheet> list = new List<SpriteSheet>();

        list.Add(basic);

        list.Add(chainHead);
        list.Add(chainNode);
        list.Add(chainPath);

        list.Add(dragHead);
        list.Add(dragCurve);

        list.Add(holdHead);
        list.Add(holdTrail);
        list.Add(holdTrailEnd);
        list.Add(holdOngoingTrail);
        list.Add(holdOngoingTrailEnd);

        list.Add(repeatHead);
        list.Add(repeat);
        list.Add(repeatHoldTrail);
        list.Add(repeatHoldTrailEnd);
        list.Add(repeatPath);

        return list;
    }
}

[Serializable]
[FormatVersion(VfxSkin.kVersion, typeof(VfxSkin), isLatest: true)]
public class VfxSkinBase : Serializable<VfxSkinBase> { }

[Serializable]
public class VfxSkin : VfxSkinBase
{
    public const string kVersion = "1";

    // VFX skin's name is the folder's name.

    public SpriteSheetForVfx feverOverlay;

    public SpriteSheetForVfx basicMax;
    public SpriteSheetForVfx basicCool;
    public SpriteSheetForVfx basicGood;

    public SpriteSheetForVfx dragOngoing;
    public SpriteSheetForVfx dragComplete;

    public SpriteSheetForVfx holdOngoingHead;
    public SpriteSheetForVfx holdOngoingTrail;
    public SpriteSheetForVfx holdComplete;

    public SpriteSheetForVfx repeatHead;
    public SpriteSheetForVfx repeatNote;
    public SpriteSheetForVfx repeatHoldOngoingHead;
    public SpriteSheetForVfx repeatHoldOngoingTrail;
    public SpriteSheetForVfx repeatHoldComplete;

    public VfxSkin()
    {
        version = kVersion;
    }

    public List<SpriteSheet> GetReferenceToAllSpriteSheets()
    {
        List<SpriteSheet> list = new List<SpriteSheet>();

        list.Add(feverOverlay);

        list.Add(basicMax);
        list.Add(basicCool);
        list.Add(basicGood);

        list.Add(dragOngoing);
        list.Add(dragComplete);

        list.Add(holdOngoingHead);
        list.Add(holdOngoingTrail);
        list.Add(holdComplete);

        list.Add(repeatHead);
        list.Add(repeatNote);
        list.Add(repeatHoldOngoingHead);
        list.Add(repeatHoldOngoingTrail);
        list.Add(repeatHoldComplete);

        return list;
    }
}