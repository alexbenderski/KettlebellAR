# 🚀 Quick Setup - Sensor Selection Panel

## ⚡ מה שצריך לעשות עכשיו:

### 1️⃣ **הגדרת SensorCardContainer**

**ב-Inspector → SensorSelectionPanel:**

```
SensorCardContainer (Transform)
├── Component: Vertical Layout Group
│   ├── Child Control Size: Width ✓, Height ✓
│   ├── Child Force Expand: Width ✓, Height ✓
│   ├── Spacing: 25
│   ├── Padding: Top=80, Bottom=40, Left=20, Right=20
│   └── Child Alignment: Upper Center
│
├── Rect Transform:
│   ├── Anchor: Middle Center Stretch
│   ├── Position: (0, 0, 0)
│   ├── Size: (full, 700)
│   └── Layout Element: Preferred Size
```

---

### 2️⃣ **יצירת SensorCard Prefab (אם עדיין לא קיים)**

**Hierarchy → Create New:**

```
SensorCard (Button - TextMeshPro) [Size: 350 × 200]
├── Icon (Image) [120×120, Center]
├── Name (TMP) [28pt, Bold, Center]
├── Description (TMP) [13pt, Gray, Center]
├── QuestionsCount (TMP) [12pt, Bottom Center]
└── Checkmark (Image) [50×50, Top Right, Green ✓]
```

**גרור ל-Assets/Prefabs כ-`SensorCardPrefab`**

---

### 3️⃣ **חיבור References ב-Inspector**

**QuizManager Component:**

```
Sensor Selection Panel:
├── Sensor Selection Panel: [גרור את ה-Panel]
├── Sensor Card Prefab: [גרור את ה-Prefab]
├── Sensor Card Container: [גרור את ה-Transform]
└── Sensor Selection Title: [TMP בתוך הפאנל]

Sensor Categories:
├── Size: 3
├── [0] Distance Sensor
│   ├── Sensor Name: "Distance Sensor"
│   ├── Sensor Description: "Ultrasonic distance measurement"
│   ├── Sensor Color: #00BCD4 (Cyan)
│   ├── Sensor Icon: [distance_icon.png]
│   ├── Badge Sprite: [distance_badge.png]
│   ├── Success Sound: [audio_clip]
│   └── Questions: [5 questions]
│
├── [1] Hub
│   ├── Sensor Name: "Hub"
│   ├── Sensor Description: "Acceleration and gyroscope"
│   ├── Sensor Color: #FF9800 (Orange)
│   ├── Sensor Icon: [hub_icon.png]
│   ├── Badge Sprite: [hub_badge.png]
│   ├── Success Sound: [audio_clip]
│   └── Questions: [5 questions]
│
└── [2] Force Sensor
    ├── Sensor Name: "Force Sensor"
    ├── Sensor Description: "Pressure and touch detection"
    ├── Sensor Color: #9C27B0 (Purple)
    ├── Sensor Icon: [force_icon.png]
    ├── Badge Sprite: [force_badge.png]
    ├── Success Sound: [audio_clip]
    └── Questions: [5 questions]
```

---

### 4️⃣ **בדיקה**

1. **Play → Press Quiz → Select Easy → צפי ל-3 כפתורים בעמודה**
   ```
   Distance Sensor (Cyan)
        ↓
      Hub (Orange)
        ↓
   Force Sensor (Purple)
   ```

2. **Console צריך להראות:**
   ```
   [QuizManager] ShowSensorSelection
   [QuizManager] CreateSensorCards - Creating 3 sensor selection buttons
   [QuizManager] Setting up sensor card: Distance Sensor
   [QuizManager] Setting up sensor card: Hub
   [QuizManager] Setting up sensor card: Force Sensor
   ```

3. **לחץ על Distance Sensor**
   ```
   [QuizManager] OnSensorSelected: Distance Sensor
   [QuizManager] Filtered 5 questions for Distance Sensor
   ```

---

## 🎨 Color Codes

| Sensor | Color | Hex | RGB |
|--------|-------|-----|-----|
| Distance | Cyan | #00BCD4 | (0, 188, 212) |
| Hub | Orange | #FF9800 | (255, 152, 0) |
| Force | Purple | #9C27B0 | (156, 39, 176) |

---

## 📍 Layout Hierarchy - Copy זה בדיוק

```
Canvas
└── QuizRoot
    ├── OverallProgressPanel (Top)
    │
    ├── LevelSelectionPanel (Hidden initially)
    │
    ├── SensorSelectionPanel (Visible on Start)
    │   ├── Title (TMP)
    │   │   └── Text: "Choose a Sensor you want to start with :"
    │   │   └── Anchor: Top Stretch
    │   │
    │   └── SensorCardContainer (Vertical Layout Group)
    │       └── [3 Cards created dynamically]
    │
    └── ... (other panels)
```

---

## ✅ Checklist

- [ ] `SensorCardContainer` has **Vertical Layout Group** (not Grid!)
- [ ] `SensorCardContainer` Spacing = 25
- [ ] `SensorCardContainer` Padding = (20, 80, 20, 40)
- [ ] `SensorCard Prefab` has: Icon, Name, Description, QuestionsCount, Checkmark
- [ ] `SensorCard Prefab` Size = 350×200
- [ ] 3 Sensor Categories defined
- [ ] All Icons assigned (Distance, Hub, Force)
- [ ] All Colors set (Cyan, Orange, Purple)
- [ ] Questions added for each sensor

---

## 🐛 Troubleshooting

### בעיה: כפתורים מרובעים או בגודל לא נכון
**פתרון:** וודא ש-Container הוא Vertical Layout Group עם `Child Force Expand: Width ✓`

### בעיה: כפתורים לא מרדפים כמו בתמונה
**פתרון:** וודא ש-SensorCard Icon הוא בגודל 120×120 ובמרכז

### בעיה: "Icon not found" בקונסול
**פתרון:** וודא ש-Prefab יש Transform child בשם `Icon` בדיוק (case-sensitive)

### בעיה: Container ריק - כפתורים לא מופיעים
**פתרון:**
1. בדוק שה-Prefab מחובר ל-Inspector
2. בדוק שה-Container מחובר ל-Inspector
3. בדוק שה-Script עדכון

---

**שלמת את הגדרה! 🎉**

עכשיו צריך להגדיר את:
- **Questions Panel** ← שאלות
- **Overall Progress Panel** ← פס התקדמות
- **Sensor Success Panel** ← אנימציית Badge
