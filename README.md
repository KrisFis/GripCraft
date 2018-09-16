# GripCraft

**Jednoduchá sandboxová "like-minecraft" hra.**
_Pro více informací o verzích_ -> **Changes.md**

**Vlastnosti vývoje:**
  - vyvíjena v UNITY engine
  - psaná v UnityScript C#
  - optimalizovaný generovaný terén

**Vlastnosti hry:**
  - Omezený frame rate na 60 fps (Problémy ve vstupu uživatele s HighEnd pc, kdy fps > 1500)
  - Možnost si vybrat kolik kostek bude předgenerováno, pro nižší trhání ve hře
  - Optimalizovaný svět pro velké množství bloků s nízkou ztrátou fps (Kombinování meshů, pro snížení počtu CPU <-> GPU volání)

**Vlastnosti světa:**
  - 4 druhy kostek (Tráva, Hlína, Písek, Kámen) s rozdílnými tvrdostmi
  - Přírodně náhodný terén
  - Chování a generování světa v reálném čase

**Vlastnosti hráče:**
  - Vejde se do díry o výšce 2 bloky a sám je široký méně než jeden block

**Ovládání hráče:**
  - Chůze (W/S/A/D nebo ŠIPKY NAHORU/DOLU/DOLEVA/DOPRAVA)
  - Běh (Chůze + LEVÝ SHIFT)
  - Stavění bloků (LEVÉ TLAČÍTKO MYŠI)
  - Ničení bloků (PRAVÉ TLAČÍTKO MYŠI)
  - Výběr bloků (1/2/3/4 nebo KOLEČKO MYŠI NAHORU/DOLU)
