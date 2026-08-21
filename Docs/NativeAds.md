# Native Ads — Kai Rite Kaam Kare Che

> Aa document non-technical che. Game ma native ad kya dekhai, API kai rite control kare,
> ane AdMob na ad serving limit thi bachva mate su su karyu — badhu saral bhasha ma.

---

## 1. Native ad game ma kya dekhai che

Total **3 jagya** par native ad muki che.

| Kya | Exactly kya position | Kai ad unit vaparai |
|---|---|---|
| **Home screen** | Bottom bar ni **niche** (bottom bar upar rahe, ad ena niche) | `native_adunit_id` |
| **Play screen** | Hint ane Restart button ni **niche** | `native_adunit_id` |
| **Win dialog** (level pura thay) | Card ni niche | `preload_native_adunit_id` |
| **Lose dialog** (level fail thay) | Card ni niche | `preload_native_adunit_id` |

### Win ane Lose ad "preload" kem che?

Etle me **level shuru thay tyare j** ad agavu magi lidhi che. Level pura
thay ke fail thay, e ad **turant** dekhai jai — koi loading, koi wait nahi.

Ane jo e ad na aavi hoy (network kharab, ke ad na mali), to Win/Lose par **kai j nahi dekhai** —
tya nava ad ni request nathi karta, karan ke e request no koi matlab nathi.
Win/Lose screen par player **rukto nathi** — e turant Next ke Retry dabave che Etle e vakhte
ad mangvani shuru kariye to ad aavse tya sudhi player nikli gayo hoy

---

## 2. API su su control kare che

Game shuru thay tyare aa API call thay che:

```
https://newvids.xyz/arrwgmln/gapp/strtdata
```

API je jawab aape, e pramane game ni **badhi ad settings** nakki thay. Code ma kai hardcode nathi —
badhu API na hath ma che.

### API na jawab ma je aave che

| Parameter | Su kaam kare |
|---|---|
| `ad_enable` | **Master switch.** `1` = badhi ad chalu. Biju kai pan (0, 2, blank) = **badhi ad bandh** |
| `native_adunit_id` | Home ane Play screen ni ad no ID |
| `preload_native_adunit_id` | Win ane Lose dialog ni ad no ID |
| `main_screen` | Home screen ni ad ni **size** |
| `play_screen` | Play screen ni ad ni **size** |
| `success_screen` | Win/Lose ni ad ni **size** |
| `refresh_native_sec` | Home ane Play ni ad **ketli second pachi** navi aave |

### Size no digit — 1 ke 2

| Digit | Kevi ad | Kevi dekhay |
|---|---|---|
| **1** | Nani, patli strip | Icon + title + ek line + nano button. Jagya bahu ochi roke |
| **2** | Moti | Icon + title + description + **moto image** + full-width Install button |
| **Biju kai pan** (3, 4, 5, 0...) | **Ad nahi** | Ane khali jagya pan nahi — screen bilkul evi j dekhai jem ad j nathi |

> **Important:** Atyare API `success_screen: 5` mokle che. `5` valid size nathi, etle
> **Win/Lose par ad nahi dekhai**. Backend ma ene `1` ke `2` karvu padse.

### Ad unit ID blank hoy to?

Ahiya logic bahu specific che:

- `ad_enable = 1` che pan **native ID blank** che → **fakt native ad bandh**. Banner ane
  interstitial chalu rahe.
- Etle ek ad na aavvathi biji ad par koi asar nathi. Darek ad potani rite chale.

### Khali jagya kadi nathi dekhati

Aa ek important UI rule che. Ad na hoy — koi pan karan hoy (ad_enable 0, ID blank, size digit
khoto, ad load na thay, network nahi) — to game **khali jagya pan nathi dekhadti**. Button ane
bottom bar potani normal jagya par j rahe.

Etle player ne kadi "ahiya kai khali kem che?" evu nathi lagtu. Ad hoy to jagya khule, na hoy to
screen normal — bas.

### Internet na hoy to?

- API no jawab game **phone ma save** kari le che.
- Next time internet na hoy to **e j saved settings** vaparai — junu pan asli settings.
- Ane jo **ek pan var** API no jawab na aavyo hoy → **ek pan ad nahi dekhai**. Game ad vagar
  normal chale.

---

## 3. AdMob ni Serving Limit thi bachva mate su karyu

### Pehla samjhie — problem su che

AdMob ma game ad **mange** che, ane Google ad **aape** che. Pan Google aa hisab rakhe che:

- **Fill rate / Match rate** — 100 var ad mangi, ketli var kharekhar mali?
- **Show rate** — je ad mali, ketli kharekhar player ne dekhai?
- **Invalid clicks** — bhul thi thayela click ketla?

Jo game **ghani ad mange pan ochi dekhade**, ke **bhul thi click** thata hoy, to Google account
par **"ad serving limit"** lagave che — matlab ad aapvanu ochu kari de ke bandh kari de.

Etle aa 9 vastu imeplement kari che.

---

### 🛡️ 1. Ad na male to vaar vaar nathi mangta (backoff)

**Problem:** ad na male ane game dar 30 second e fari fari mange, to ek kalak ma 120 request
ane 0 ad. Google mate aa sauthi kharab signal che.

**Solution:** fail thay to game **ruke che**, ane dar var vadhu ruke che:

```
1st fail  →  30 second pachi try
2nd fail  →  60 second pachi
3rd fail  →  2 minute pachi
4th fail  →  4 minute pachi
5th+ fail →  5 minute pachi (aa max che)
```

Ek var ad mali jai, to badhu **reset** — fari normal 30 second par aavi jai.

Etle je phone ma ad j nathi malti (jem ke koi country ma inventory nathi), tya game shanti thi
ochi request kare.

### 🛡️ 2. Pehla jagya, pachi ad (accidental click thi bachav)

**Problem:** Native ad Android pote game ni **upar** dore che. Jo ad screen par aavi jai ane
game na button haji potani juni jagya par hoy, to ad **button ni upar** besi jai. Player Hint
dabave, pan tap **ad par** lage → **bhul thi click**.

Bhul thi click = invalid traffic.

**Solution:** order ulto kari nakhyo:

```
1. Game pehla jagya khole (button upar khase)
2. Layout set thay
3. PACHI ad dekhai
```

Ane jo ad potani size na reporte (bahu rare), to pan game **jagya rakhi mukse** — ad ne
button upar besvanu mauko j nathi apto.

### 🛡️ 3. Win/Lose par live request nahi

Upar samjhavyu — Win/Lose par fakt **agavu magelu** ad j dekhai. Tya nava ad ni request kadi
nathi thati. Etle je request no koi impression na thay, e request thati j nathi.

### 🛡️ 4. Preload level ma **ek j var**

Win dialog par ad dekhadyu, ke turant next ad magvanu? **Nahi.** Player tya thi quit kari de to
e request vestu jay.

Preload **fakt level shuru thay tyare** thay che. Ek level = ek request. Bas.

### 🛡️ 5. Juni ad kadi nathi dekhadta (30 minute limit)

Google cache ma padeli ad ne **ek kalak** pachi "stale" gane che. Juni ad dekhadvi = policy violence kharab data.

Me **30 minute** ni limit rakhi che (ek kalak thi saari andar). Level bahu lambu chale ane
preload karel ad 30 minute juni thai jai, to e **feki dai** — ane Win/Lose par kai nahi dekhai.

Aa number Inspector ma **badli sakay** che, jarur pade tyare.

### 🛡️ 6. Test device register karvani vyavastha

**Problem:** developer pota na phone par asli (live) ad par click kare → Google mate e **invalid
click** che, ane e **tamara account** par count thay.

**Solution:** phone no test-device ID settings ma nakhi sakay che. Pachi e phone ne Google
"test" gane — click count nathi thata, account safe.

### 🛡️ 7. Development ma Google na test ad

Atyare game **test mode** ma che — Google na official test ad unit vaparai che, asli ad unit ne
adkto j nathi. Etle testing ma account ne koi risk nathi.

> **Release pehla aa off karvu jaruri che.** Bhulai jai to real player ne test ad dekhai ane
> Etle code ma **moto red error log** mukyo che je dar var kahe:
> *"TEST AD UNITS ARE ON. Turn off before release."*

### 🛡️ 8. Number game mathi j joi sakay (counters)

Have game pote hisab rakhe che ane log ma lakhe che:

```
requests 12, filled 11 (92% match), impressions 10 (91% shown), clicks 0, revenue 0.0043
```

Etle AdMob dashboard ni vaat joya vagar pan turant khabar pade ke request vadhu ane impression
ochi thai rahi che ke nahi — problem thata pehla pakadi sakay.

### 🛡️ 9. Screen chodo to ad turant band

Native ad game ni upar dorai che, etle ene chhodi mukiye to e **next screen sudhi follow** kare.
Jem ke Play screen ni ad Win dialog upar besi jay.

Etle darek screen chodta j potani ad band kari de che. Ek time par ek j ad, ane e barabar potani
jagya par.

---

## 4. Refresh kai rite kaam kare

| Screen | Refresh thay? |
|---|---|
| Home screen | **Ha** — API ni `refresh_native_sec` pramane |
| Play screen | **Ha** — API ni `refresh_native_sec` pramane |
| Win / Lose dialog | **Nahi** — dialog be second nu che, refresh no koi matlab nathi |

### Screen chodi ne pacha aavo tyare

Dakhla tarike, refresh 30 second che:

1. Home screen khulyu → ad aavi, 30 second no timer chalu
2. 15 second pachi player Settings ma gayo → ad band, timer band
3. Player pacho Home ma aavyo → **navi ad** aavi, timer **fari 30 second thi** shuru

Timer 15 second thi aage nathi chalto. Dar var Home khule, navi ad ane puro navo timer.

> **Note:** API atyare `refresh_native_sec: 10` mokle che. 10 second bahu jaldi che — ek kalak ma
> ghani request ane darek ad bahu ochi var dekhai, je match rate ne nukshan kare. **30 second**
> recommend karie chie (AdMob ni potani banner guidance pan 30-60 second ni che).

---

## 5. Ek najar ma — badha rules

Ad dekhadta pehla game aa **badhu** check kare che. Ek pan fail thay → ad nahi, ane khali jagya
pan nahi:

```
1. API no jawab aavyo che? (ke phone ma saved che?)      →  na hoy to ad nahi
2. ad_enable = 1 che?                                     →  na hoy to badhi ad bandh
3. Aa ad no unit ID che?                                  →  blank hoy to FAKT aa ad bandh
4. Size digit 1 ke 2 che?                                 →  biju hoy to FAKT aa jagya band
5. Win/Lose hoy to — agavu magel ad hath ma che?           →  na hoy to ad nahi
6. E ad 30 minute thi juni nathi?                          →  juni hoy to ad nahi
```

---

## 6. Client ne nakki karvanu — 3 vastu

| # | Su | Kem jaruri |
|---|---|---|
| 1 | `success_screen` ne **`1` ke `2`** karvu | Atyare `5` che, je valid nathi — etle **Win/Lose par ad j nahi dekhai** |
| 2 | `refresh_native_sec` ne **`30`** karvu | Atyare `10` che — bahu jaldi, match rate ne nukshan |
| 3 | Release pehla **test mode off** | Nahi to real player ne test ad dekhai, kamai zero |

Ane ek suggestion: `main_screen` ane `play_screen` mate —

- **Play screen** par **`1`** (nani strip) sari rahe, karan ke tya game chale che ane moti ad
  jagya bahu roke
- **Home screen** par **`2`** (moti) sari rahe, karan ke tya jagya che ane moti ad ni kamai vadhu
  hoy che

Pan aa client ni marji — API ma digit badle etle game ma turant asar thay, koi update jaruri nahi.
