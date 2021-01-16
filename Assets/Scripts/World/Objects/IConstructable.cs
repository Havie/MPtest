using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IConstructable : IMoveable
{

    void SetHandPreviewingMode(bool cond);

    void ChangeAppearanceHidden(bool cond);

    void ChangeAppearancePreview();
}
