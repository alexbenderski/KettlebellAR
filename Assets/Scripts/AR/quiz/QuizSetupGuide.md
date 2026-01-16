# 📚 מדריך הגדרת מערכת Quiz ב-Unity

## 🎯 סקירה כללית

המערכת החדשה כוללת:
1. **מסך בחירת רמת קושי** - Easy / Medium / Hard
2. **מסך בחירת חיישן** - עם 3 קטגוריות (Distance, Force, Hub)
3. **פס התקדמות כללי** - אחוזים לכל חיישן
4. **מערכת Badges** - Badge לאחר 3+ תשובות נכונות
5. **מסך סיום** - כל ה-Badges + כפתור התחלה מחדש

---

## 📂 שלב 1: יצירת UI Hierarchy

### 1.1 מבנה האובייקטים ב-Unity

```
Canvas (QuizRoot)
│
├── 🎚️ LevelSelectionPanel
│   ├── Title (TMP) - "Select Difficulty Level"
│   ├── EasyButton (Button)
│   │   └── Text (TMP) - "Easy"
│   ├── MediumButton (Button)
│   │   └── Text (TMP) - "Medium"
│   └── HardButton (Button)
│       └── Text (TMP) - "Hard"
│
├── 📊 OverallProgressPanel (מוצג בכל המסכים)
│   ├── ProgressTitle (TMP) - "Overall Progress"
│   ├── ProgressText (TMP) - "35% Complete - Keep learning!"
│   ├── OverallProgressBar (Slider)
│   │   ├── Background
│   │   └── Fill (Gradient: Green → Yellow → Red)
│   ├── ProgressPercent (TMP) - "35%"
│   ├── MilestoneMarkers
│   │   ├── Marker_25% (Image)
│   │   ├── Marker_50% (Image)
│   │   └── Marker_75% (Image)
│   ├── EarnedBadgesContainer (Horizontal Layout)
│   │   └── [Dynamic] BadgeItems
│   └── SensorProgressContainer (Horizontal Layout)
│       └── [Dynamic] SensorProgressPrefabs
│
├── 🎯 SensorSelectionPanel
│   ├── Title (TMP) - "Select Sensor"
│   ├── SensorCardContainer (Grid Layout 2x2)
│   │   ├── [Dynamic] SensorCard_Distance
│   │   ├── [Dynamic] SensorCard_Force
│   │   └── [Dynamic] SensorCard_Hub
│   └── BackButton
│
├── ❓ QuestionPanel
│   ├── SensorHeader
│   │   ├── CurrentSensorIcon (Image)
│   │   └── CurrentSensorName (TMP)
│   ├── QuestionProgressBar (Slider)
│   ├── QuestionCounter (TMP) - "Question 3/5"
│   ├── QuestionText (TMP)
│   ├── AnswerButtonsContainer
│   │   ├── AnswerA (Button)
│   │   ├── AnswerB (Button)
│   │   ├── AnswerC (Button)
│   │   └── AnswerD (Button)
│   └── BackButton
│
├── 🎉 SensorSuccessPanel
│   ├── Background (Dark Overlay)
│   ├── SuccessBadge (Image + Animator)
│   ├── SuccessMessage (TMP) - "You earned the Badge!"
│   ├── SuccessSubMessage (TMP) - "Score: 4/5"
│   └── ContinueButton (Button)
│
└── 🏆 FinalCompletionPanel
    ├── Background
    ├── FinalTitle (TMP) - "SPIKE Expert!"
    ├── FinalBadgeContainer (Horizontal Layout)
    │   └── [Dynamic] BadgeItems
    ├── FinalMessage (TMP)
    ├── RestartButton (Button) - "Play Again"
    └── BackToMenuButton (Button) - "Main Menu"
```

---

## 🔧 שלב 2: יצירת Prefabs

### 2.1 SensorCard Prefab (כרטיס חיישן)

**Hierarchy → Right Click → UI → Button - TextMeshPro**

```
SensorCard (Button) [Size: 400x250]
├── Background (Image)
│   └── Color: White with rounded corners
│
├── Icon (Image) [Size: 80x80]
│   └── Anchor: Top Center
│   └── Position: Y = 60
│
├── Name (TMP) [Font: Bold, Size: 28]
│   └── Anchor: Middle Center
│   └── Text: "Distance Sensor"
│
├── Description (TMP) [Font: Regular, Size: 16]
│   └── Anchor: Middle Center
│   └── Text: "Ultrasonic distance measurement"
│   └── Color: Gray
│
├── QuestionsCount (TMP) [Size: 18]
│   └── Anchor: Bottom Center
│   └── Text: "5 questions"
│   └── Background: Pill-shaped
│
└── Checkmark (Image) [Size: 40x40]
    └── Anchor: Top Right
    └── Sprite: Green Checkmark
    └── SetActive: false
```

**📍 שמירה:**
1. גרור `SensorCard` לתיקיית `Assets/Prefabs`
2. מחק מה-Hierarchy

---

### 2.2 EarnedBadge Prefab (Badge קטן לפס התקדמות)

```
EarnedBadge (Image) [Size: 60x60]
└── Badge sprite (dynamic)
└── Add Animator component for pop effect
```

**📍 שמירה:** `Assets/Prefabs/EarnedBadgePrefab`

---

### 2.3 SensorProgress Prefab (פס התקדמות לחיישן בודד)

```
SensorProgress (GameObject) [Size: 200x50]
├── Name (TMP) [Size: 14]
│   └── Text: "Distance"
│
├── ProgressSlider (Slider) [Height: 10]
│   └── Min: 0, Max: 1
│   └── Fill Color: Sensor color
│
└── Percent (TMP) [Size: 12]
    └── Text: "60%"
```

**📍 שמירה:** `Assets/Prefabs/SensorProgressPrefab`

---

### 2.4 FinalBadge Prefab (Badge למסך סיום)

```
FinalBadge (Image) [Size: 120x150]
├── BadgeImage (Image) [Size: 100x100]
│   └── Sprite: dynamic
│
└── SensorName (TMP) [Size: 16]
    └── Text: "Distance Sensor"
    └── Alignment: Center
```

**📍 שמירה:** `Assets/Prefabs/FinalBadgePrefab`

---

## 🎚️ שלב 3: יצירת Level Selection Panel

### 3.1 יצירה ב-Unity

1. **Hierarchy → Right Click → UI → Panel**
   - שם: `LevelSelectionPanel`
   - Anchor: Stretch All
   - Color: Background color (dark blue/gradient)

2. **תחתיו הוסף:**

#### Title Text:
```
Right Click → UI → Text - TextMeshPro
Name: LevelTitleText
Text: "Select Difficulty Level"
Font Size: 42
Alignment: Center
Anchor: Top Center
Position Y: -100
```

#### Buttons Container:
```
Right Click → Create Empty
Name: ButtonsContainer
Add Component → Vertical Layout Group
  - Spacing: 30
  - Child Force Expand: Width ✓
Anchor: Middle Center
```

#### Easy Button:
```
Right Click → UI → Button - TextMeshPro
Name: EasyButton
Size: (300, 80)
Image Color: #4CAF50 (Green)
Text: "🌱 Easy"
Font Size: 28
```

#### Medium Button:
```
Name: MediumButton
Image Color: #FF9800 (Orange)
Text: "⚡ Medium"
```

#### Hard Button:
```
Name: HardButton
Image Color: #F44336 (Red)
Text: "🔥 Hard"
```

---

## 📊 שלב 4: יצירת Overall Progress Panel

### 4.1 מיקום
- **Anchor: Top Stretch** (למעלה, לכל הרוחב)
- **Height: 180**
- **מופיע בכל המסכים**

### 4.2 רכיבים

#### Progress Title:
```
TMP: "Overall Progress"
Position: Top Left
Font Size: 18
Color: White
```

#### Progress Text:
```
TMP: "35% Complete - Keep learning!"
Position: Below title
Font Size: 16
Color: Light Gray
```

#### Overall Progress Bar:
```
Slider
Width: Full - padding
Height: 25
Colors:
  - Background: #E0E0E0
  - Fill: Gradient (Green #4CAF50 → Red via Yellow)
Value: 0
```

#### Milestone Markers:
```
4 נקודות על הפס:
- Start (0%)
- 25%
- 50%
- 75%
- Done (100%)
```

#### Earned Badges Container:
```
Horizontal Layout Group
Position: Above progress bar
Child Alignment: Middle Left
Spacing: 10
```

---

## 🎯 שלב 5: יצירת Sensor Selection Panel (3 כפתורים אנכיים)

### 5.1 מבנה - עיצוב חדש עם 3 כפתורים בעמודה

```
SensorSelectionPanel (Panel)
├── Background (Blue gradient - כמו בתמונה)
│   └── Image color: #1A4F7A (Dark Blue)
│
├── Title (TMP) - בחלק העליון
│   ├── Text: "Choose a Sensor you want to start with :"
│   ├── Font Size: 24
│   ├── Color: White
│   ├── Anchor: Top Stretch
│   └── Height: 60
│
└── SensorCardContainer (Vertical Layout Group) ⭐ חשוב!
    ├── Anchor: Middle Center Stretch
    ├── Height: Preferred Size
    ├── Settings:
    │   - Child Control Size: Height ✓, Width ✓
    │   - Child Force Expand: Height ✓, Width ✓
    │   - Spacing: 25 (בין כפתורים)
    │   - Padding: Top 80, Bottom 40, Left 20, Right 20
    │   - Child Alignment: Upper Center
    │
    └── 3 Cards (סידור אנכי, כל אחד 350x200):
        ├── SensorCard_Distance Sensor (ראשון)
        ├── SensorCard_Hub (שני)
        └── SensorCard_Force Sensor (שלישי)
```

### 5.2 SensorCard Prefab - סדרה חדשה

**הכרטיס חייב להיות בנוי כך:**

```
SensorCard (Button) 
├── Rect Transform:
│   ├── Size: (350, 200)
│   ├── Anchor: Middle Center
│   └── Layout Element: Preferred Size
│
├── Image (Background)
│   ├── Anchor: Stretch All
│   ├── Color: [Dynamic - based on sensor]
│   │   ├── Distance: #00BCD4 (Cyan)
│   │   ├── Hub: #FF9800 (Orange)
│   │   └── Force: #9C27B0 (Purple)
│   └── Opacity: 80% (active), 40% (completed)
│
├── Icon (Image) - גדול ובמרכז!
│   ├── Rect Transform: (120, 120)
│   ├── Anchor: Middle Center
│   ├── Position: (0, 30) - עדיף מעט למעלה
│   └── Sprite: [Dynamic sensor icon]
│
├── Name (TMP) - מתחת לאייקון
│   ├── Rect Transform: (300, 50)
│   ├── Anchor: Middle Center
│   ├── Position: (0, -40)
│   ├── Text: "Distance Sensor"
│   ├── Font Size: 28
│   ├── Font Style: Bold
│   ├── Alignment: Center Midline
│   ├── Color: White
│   ├── Overflow: Ellipsis
│   └── Rich Text: Enabled
│
├── Description (TMP) - קטן יותר
│   ├── Rect Transform: (320, 40)
│   ├── Anchor: Middle Center
│   ├── Position: (0, -80)
│   ├── Text: "Ultrasonic distance measurement"
│   ├── Font Size: 13
│   ├── Alignment: Center
│   ├── Color: #CCCCCC (Light Gray)
│   └── Overflow: Ellipsis
│
├── QuestionsCount (TMP) - כמו בתמונה
│   ├── Rect Transform: (100, 30)
│   ├── Anchor: Bottom Center
│   ├── Position: (0, 10)
│   ├── Text: "5 questions"
│   ├── Font Size: 12
│   ├── Alignment: Center
│   ├── Color: White / Semi-transparent
│   └── Background: Optional rounded pill shape
│
└── Checkmark (Image) - V ירוק
    ├── Rect Transform: (50, 50)
    ├── Anchor: Top Right
    ├── Position: (-10, -10)
    ├── Sprite: Green Checkmark ✓ (or star)
    └── SetActive: false (shown when completed)
```

### 5.3 Unity Setup - שלב אחרי שלב

#### שלב 1: יצירת SensorCard Prefab

1. **Hierarchy → UI → Button - TextMeshPro**
   - שם: `SensorCard`
   - Size: 350 × 200

2. **תחת SensorCard הוסף:**
   
   **Icon:**
   ```
   Right Click → UI → Image
   Name: Icon
   Size: 120 × 120
   Anchor: Middle Center
   Sprite: [leave empty - will be set dynamically]
   ```

   **Name:**
   ```
   Right Click → UI → TextMeshPro
   Name: Name
   Text: "Distance Sensor"
   Font Size: 28
   Anchor: Middle Center
   Position: (0, -40)
   ```

   **Description:**
   ```
   Right Click → UI → TextMeshPro
   Name: Description
   Text: "Ultrasonic distance measurement"
   Font Size: 13
   Anchor: Middle Center
   Position: (0, -80)
   Color: #CCCCCC
   ```

   **QuestionsCount:**
   ```
   Right Click → UI → TextMeshPro
   Name: QuestionsCount
   Text: "5 questions"
   Font Size: 12
   Anchor: Bottom Center
   Position: (0, 10)
   ```

   **Checkmark:**
   ```
   Right Click → UI → Image
   Name: Checkmark
   Size: 50 × 50
   Anchor: Top Right
   Position: (-10, -10)
   Sprite: Green check ✓
   SetActive: false
   ```

3. **גרור את SensorCard לתיקיית `Assets/Prefabs`**
4. **מחק מה-Hierarchy**

#### שלב 2: הגדרת Container

1. **Hierarchy → SensorSelectionPanel → Create Empty**
   - שם: `SensorCardContainer`

2. **Component → Vertical Layout Group**
   - Settings:
     - ✓ Child Control Size: Width, Height
     - ✓ Child Force Expand: Width, Height
     - Spacing: 25
     - Padding: Top 80, Bottom 40, Left 20, Right 20
     - Child Alignment: Upper Center

3. **Rect Transform:**
   - Anchor: Middle Center Stretch
   - Width: Full
   - Height: 700 (approximately)

---

## ❓ שלב 6: יצירת Question Panel

### 6.1 Sensor Header
```
Container at top
├── CurrentSensorIcon (Image, 50x50)
└── CurrentSensorName (TMP, Bold, Size 24)
```

### 6.2 Question Progress
```
QuestionProgressBar (Slider)
  - Width: Full
  - Height: 8
  - Fill: Blue gradient
  
QuestionCounter (TMP)
  - "Question 3/5"
  - Position: Right of slider
```

### 6.3 Question Text
```
QuestionText (TMP)
  - Font Size: 28
  - Alignment: Center
  - Text Area: Full width
  - RTL Support: RTLTextMeshPro if needed
```

### 6.4 Answer Buttons
```
Vertical Layout Group
Spacing: 15

Each Button:
  - Size: (Full width - padding, 70)
  - Normal Sprite: White/Light
  - Correct Sprite: Green
  - Wrong Sprite: Red
```

---

## 🎉 שלב 7: יצירת Sensor Success Panel

### 7.1 מבנה

```
SensorSuccessPanel (Panel)
├── DarkOverlay (Image, Black 80% alpha)
│
├── ContentContainer (Centered)
│   ├── SuccessBadge (Image, 200x200)
│   │   └── Animator: BadgePopup
│   │
│   ├── SuccessMessage (TMP)
│   │   └── "🎉 Amazing! 🎉\n\nYou earned the\nDistance Sensor Badge!"
│   │
│   ├── SuccessSubMessage (TMP)
│   │   └── "Score: 4/5 correct answers"
│   │
│   └── ContinueButton (Button)
│       └── "Continue"
│
└── ParticleSystem (Optional: Confetti)
```

### 7.2 Animation Setup

1. **Window → Animation → Animation**
2. בחר `SuccessBadge`
3. **Create → BadgePopup.anim**
4. Keyframes:
   - Frame 0: Scale (0, 0, 1)
   - Frame 15: Scale (1.3, 1.3, 1)
   - Frame 25: Scale (1, 1, 1)
5. **Animator → Parameters → Add Trigger → "Show"**
6. Transition: Entry → BadgePopup (Trigger: Show)

---

## 🏆 שלב 8: יצירת Final Completion Panel

### 8.1 מבנה

```
FinalCompletionPanel (Panel)
├── Background (Gradient/Celebration image)
│
├── FinalTitle (TMP)
│   └── "🏆 SPIKE Prime Expert! 🏆"
│   └── Font Size: 48
│
├── FinalBadgeContainer (Horizontal Layout)
│   └── Spacing: 20
│   └── Child Alignment: Middle Center
│
├── FinalMessage (TMP)
│   └── Multi-line feedback text
│   └── Font Size: 22
│
├── ButtonsContainer (Horizontal Layout)
│   ├── RestartButton
│   │   └── "🔄 Play Again"
│   │   └── Color: Green
│   │
│   └── BackToMenuButton
│       └── "🏠 Main Menu"
│       └── Color: Blue
│
└── Particles (Optional celebration)
```

---

## 📝 שלב 9: הגדרת Sensor Categories ב-Inspector

### 9.1 פתיחת QuizManager ב-Inspector

1. בחר את ה-GameObject עם `QuizManager`
2. מצא את השדה `Sensor Categories`
3. הגדר **Size = 3**

### 9.2 Distance Sensor (Element 0)

| Field | Value |
|-------|-------|
| Sensor Name | `Distance Sensor` |
| Sensor Description | `Ultrasonic distance measurement` |
| Sensor Color | `Cyan (#00BCD4)` |
| Sensor Icon | גרור `distance_icon.png` |
| Badge Sprite | גרור `distance_badge.png` |
| Success Sound | גרור `distance_success.mp3` |
| Questions | Size = 5 (ראה למטה) |

### 9.3 Force Sensor (Element 1)

| Field | Value |
|-------|-------|
| Sensor Name | `Force Sensor` |
| Sensor Description | `Pressure and touch detection` |
| Sensor Color | `Orange (#FF9800)` |

### 9.4 Hub (Element 2)

| Field | Value |
|-------|-------|
| Sensor Name | `Hub` |
| Sensor Description | `Acceleration and gyroscope` |
| Sensor Color | `Purple (#9C27B0)` |

---

## ❓ שלב 10: הוספת שאלות (Questions)

### 10.1 שאלות Distance Sensor

```
Question 0:
├── Question: "What does the Distance Sensor measure?"
├── Answers: ["Distance to objects", "Temperature", "Light intensity", "Sound level"]
├── Correct Index: 0
└── Difficulty: Easy

Question 1:
├── Question: "What technology does the Distance Sensor use?"
├── Answers: ["Ultrasonic waves", "Laser beam", "Infrared light", "Radio waves"]
├── Correct Index: 0
└── Difficulty: Easy

Question 2:
├── Question: "What is the maximum range of the SPIKE Distance Sensor?"
├── Answers: ["200 cm", "50 cm", "500 cm", "1000 cm"]
├── Correct Index: 0
└── Difficulty: Medium

Question 3:
├── Question: "In the Kettlebell exercise, what does the Distance Sensor measure?"
├── Answers: ["Jump height", "Body weight", "Heart rate", "Running speed"]
├── Correct Index: 0
└── Difficulty: Medium

Question 4:
├── Question: "Why should you avoid rugs when using the Distance Sensor?"
├── Answers: ["Soft surfaces absorb sound waves", "They generate static", "They block infrared", "They reflect too much"]
├── Correct Index: 0
└── Difficulty: Hard
```

### 10.2 שאלות Force Sensor

```
Question 0:
├── Question: "What does the Force Sensor detect?"
├── Answers: ["Pressure and touch", "Distance", "Color", "Temperature"]
├── Correct Index: 0
└── Difficulty: Easy

Question 1:
├── Question: "How many levels of force can the sensor detect?"
├── Answers: ["Multiple levels (0-10 Newtons)", "Only on/off", "3 levels", "100 levels"]
├── Correct Index: 0
└── Difficulty: Medium

Question 2:
├── Question: "What unit does the Force Sensor measure in?"
├── Answers: ["Newtons", "Kilograms", "Pounds", "Joules"]
├── Correct Index: 0
└── Difficulty: Medium

Question 3:
├── Question: "The Force Sensor can detect which of the following?"
├── Answers: ["Both push and touch", "Only push", "Only pull", "Only vibration"]
├── Correct Index: 0
└── Difficulty: Easy

Question 4:
├── Question: "What happens when you press harder on the Force Sensor?"
├── Answers: ["Higher value reading", "Lower value reading", "Same reading", "Sensor turns off"]
├── Correct Index: 0
└── Difficulty: Easy
```

### 10.3 שאלות Hub

```
Question 0:
├── Question: "What sensor is built into the SPIKE Hub?"
├── Answers: ["Accelerometer/Gyroscope", "Camera", "GPS", "Microphone"]
├── Correct Index: 0
└── Difficulty: Easy

Question 1:
├── Question: "The Hub's gyroscope measures what?"
├── Answers: ["Rotation and orientation", "Distance", "Force", "Temperature"]
├── Correct Index: 0
└── Difficulty: Easy

Question 2:
├── Question: "In the squat jump experiment, what does the Hub measure?"
├── Answers: ["Acceleration during jump", "Body temperature", "Sound of landing", "Light in room"]
├── Correct Index: 0
└── Difficulty: Medium

Question 3:
├── Question: "How should the Hub be held during jump measurements?"
├── Answers: ["Perpendicular to ground", "Flat on ground", "Above head", "Behind back"]
├── Correct Index: 0
└── Difficulty: Medium

Question 4:
├── Question: "What is Potential Energy formula that relates to jump height?"
├── Answers: ["Ep = mgh", "E = mc²", "F = ma", "P = IV"]
├── Correct Index: 0
└── Difficulty: Hard
```

---

## 🔗 שלב 11: חיבור References ב-Inspector

### 11.1 רשימת כל ה-References

| Section | Field | גרור מ- |
|---------|-------|--------|
| **Level Selection** | Level Selection Panel | LevelSelectionPanel |
| | Easy Button | EasyButton |
| | Medium Button | MediumButton |
| | Hard Button | HardButton |
| | Level Title Text | LevelTitleText |
| **Sensor Selection** | Sensor Selection Panel | SensorSelectionPanel |
| | Sensor Card Prefab | Assets/Prefabs/SensorCard |
| | Sensor Card Container | SensorCardContainer |
| | Sensor Selection Title | Title TMP |
| **Overall Progress** | Overall Progress Panel | OverallProgressPanel |
| | Overall Progress Bar | Slider |
| | Overall Progress Text | ProgressText |
| | Overall Progress Percent | PercentText |
| | Sensor Progress Container | SensorProgressContainer |
| | Sensor Progress Prefab | Assets/Prefabs/SensorProgressPrefab |
| **Badge Display** | Earned Badges Container | EarnedBadgesContainer |
| | Earned Badge Prefab | Assets/Prefabs/EarnedBadgePrefab |
| **Question Panel** | Question Panel | QuestionPanel |
| | Question Text | QuestionText |
| | Question Counter Text | QuestionCounter |
| | Answer A-D | Buttons |
| | Text A-D | Button texts |
| | Question Progress Bar | QuestionProgressBar |
| | Current Sensor Icon | SensorHeaderIcon |
| | Current Sensor Name | SensorHeaderName |
| **Button Sprites** | Normal Sprite | white_button.png |
| | Correct Sprite | green_button.png |
| | Wrong Sprite | red_button.png |
| **Sensor Success** | Sensor Success Panel | SensorSuccessPanel |
| | Success Badge | BadgeImage |
| | Success Message | SuccessMessage |
| | Success Sub Message | SuccessSubMessage |
| | Success Animator | Animator component |
| | Continue Button | ContinueButton |
| **Final Completion** | Final Completion Panel | FinalCompletionPanel |
| | Final Badge Container | FinalBadgeContainer |
| | Final Badge Prefab | Assets/Prefabs/FinalBadgePrefab |
| | Final Title Text | FinalTitle |
| | Final Message Text | FinalMessage |
| | Restart Button | RestartButton |
| | Back To Menu Button | BackToMenuButton |
| **General UI** | Main Menu Panel | MainMenuPanel |
| | Explore Button | ExploreButton |
| | Quiz Button | QuizButton |
| | Back Button | BackButton |
| | Quiz Root | QuizRoot Canvas |
| | Background Main Menu | Background |
| **Audio** | Audio Source | AudioSource component |
| | Correct Sound | correct.mp3 |
| | Wrong Sound | wrong.mp3 |
| | Badge Earned Sound | badge_earned.mp3 |
| | All Completed Sound | celebration.mp3 |
| | Button Click Sound | click.mp3 |

---

## ✅ שלב 12: בדיקת המערכת

### 12.1 Flow Test

1. **Play → לחץ Quiz**
2. **מסך Level Selection מופיע**
3. **לחץ Easy**
4. **מסך Sensor Selection מופיע**
   - רואה 3 כרטיסי חיישנים
   - פס התקדמות ריק (0%)
5. **לחץ על Distance Sensor**
6. **מסך Questions מופיע**
   - פס התקדמות מתעדכן
   - שאלות מופיעות
7. **ענה על 3 שאלות נכון**
   - Badge מופיע בפס התקדמות!
8. **סיים את כל השאלות**
   - מסך Success מופיע
9. **לחץ Continue**
   - חוזר לבחירת חיישן
   - Distance מסומן ב-✓
10. **סיים את כל החיישנים**
    - מסך Final מופיע
    - כל ה-Badges מוצגים

### 12.2 Debug Console

צפה בהודעות:
```
[QuizManager] ShowLevelSelection
[QuizManager] OnDifficultySelected: Easy
[QuizManager] ShowSensorSelection
[QuizManager] OnSensorSelected: Distance Sensor
[QuizManager] ShowSensorSuccess: Distance Sensor - Score: 4/5
[QuizManager] ShowFinalCompletion - All sensors completed!
```

---

## 🎨 שלב 13: עיצוב נוסף (אופציונלי)

### 13.1 Gradient Progress Bar

1. **Edit → Project Settings → Graphics**
2. הוסף Gradient Shader
3. החל על Fill של ה-Slider

### 13.2 Particle Effects

```
Confetti:
  - Emission: 100 particles
  - Shape: Cone
  - Color over Lifetime: Rainbow
  - Size over Lifetime: Decrease
```

### 13.3 Sound Effects

| Event | Suggested Sound |
|-------|-----------------|
| Button Click | UI click/pop |
| Correct Answer | Ding/chime |
| Wrong Answer | Buzzer (soft) |
| Badge Earned | Fanfare/achievement |
| All Complete | Celebration music |

---

## 🐛 Troubleshooting

### בעיה: Prefab לא מופיע
**פתרון:** וודא ש-Prefab נשמר בתיקיית Assets/Prefabs

### בעיה: NullReferenceException
**פתרון:** בדוק שכל ה-References מחוברים ב-Inspector

### בעיה: כפתורים לא מגיבים
**פתרון:** וודא שה-Button component מופעל ו-Raycast Target = true

### בעיה: אנימציה לא עובדת
**פתרון:** בדוק ש-Trigger "Show" קיים ב-Animator Parameters

---

## 📋 Checklist סופי

- [ ] `LevelSelectionPanel` + 3 כפתורי קושי
- [ ] `SensorSelectionPanel` + Grid Layout
- [ ] `SensorCardPrefab` עם Icon, Name, Description, QuestionsCount, Checkmark
- [ ] `OverallProgressPanel` + Slider + Badges Container
- [ ] `EarnedBadgePrefab` + Animator
- [ ] `SensorProgressPrefab` עם Name, Slider, Percent
- [ ] `QuestionPanel` + כל רכיבי השאלות
- [ ] `SensorSuccessPanel` + Animator + ContinueButton
- [ ] `FinalCompletionPanel` + RestartButton + BackButton
- [ ] 3 Sensor Categories עם 5 שאלות כל אחד
- [ ] כל ה-Audio Clips מחוברים
- [ ] כל ה-Sprites מחוברים
- [ ] בדיקת Flow מלאה

---

## 🎉 סיום

המערכת מוכנה! עכשיו יש לך:

✅ בחירת רמות קושי
✅ 3 קטגוריות חיישנים
✅ פס התקדמות כללי עם אחוזים
✅ מערכת Badges (3+ תשובות נכונות = Badge)
✅ מסך סיום עם כל ה-Badges
✅ כפתור התחלה מחדש
✅ שאלות רלוונטיות ל-SPIKE Prime

בהצלחה! 🚀
