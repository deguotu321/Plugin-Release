Hey folks! Just finished polishing up the Reality Stabilizer plugin. Its main goal is to help out players who die early, giving them more opportunities to experience different roles and learn the map before the real chaos begins.

Core Concept: The Stabilization Phase

    At the start of every round, a random "Reality Stabilization" period begins.

    This phase lasts between 160 and 280 seconds (randomly chosen each round).

    During this time, death isn't permanent! Players who die will automatically respawn.

Key Features:

    Smart Player Respawning:

        When a player dies during stabilization, they enter a 5-second respawn countdown.

        After the countdown, they respawn at a random location.

        Role Probability on Respawn:

            55% Chance: Class D Personnel

            25% Chance: Scientist

            20% Chance: Facility Guard

        Spawn Locations (Auto-selected based on role):

            Class D / Scientist: Light Containment Zone

            Facility Guard: Heavy Containment Zone

        Respawn messages show the exact room name in English (e.g., "SCP-173 Containment Chamber").

    Temporal Disturbance Mechanism (Damage Hurts Stability!):

        Player damage destabilizes reality! Every time a player takes damage:

            The remaining stabilization time decreases by 1 second.

            A visual alert warns everyone about the time loss.

        Anti-Spam: Damage notifications have a 2.5-second cooldown to prevent chat flooding.

    Dynamic Countdown & Alerts:

        > 10 seconds left: Server-wide reminder every 20 seconds.

        ≤ 10 seconds left: Red Alert! Loud, clear countdown every second.

        Stabilization Ends: Clear server-wide notification when the phase is over.

    Highly Configurable:

        Toggle the entire stabilization system on/off.

        Adjust the initial duration range (160-280s).

        Toggle damage notifications and respawn notifications.

        Enable debug mode for testing.

How It Plays Out In-Game:

    Round Start:

        System Activates! Server message shows stabilization duration and warning.

        Random stabilization timer (160-280s) begins.

    Player Dies:

        5-second respawn countdown starts (shows role & location).

        Player respawns automatically at a random appropriate location with full health.

    Player Takes Damage:

        Temporal Disturbance! Remaining time -1 second.

        Red alert notification flashes server-wide.

    Stabilization Ends:

        System Deactivation notification.

        Normal gameplay rules resume (death is permanent again).

Under the Hood (Tech Stuff):

    Built using the EXILED framework.

    Uses coroutines for precise timing control.

    Includes anti-spam cooldowns for damage notifications.

    Robust error handling for stability.

    Full room name mapping system for accurate location display.

    Reactive UI Hints: Notifications use color-coding (red alerts!) and size changes for importance.

Why You'll Like It:

    Reduces early-round frustration: New or unlucky players get back into the action fast.

    Encourages role experimentation: Higher chance to try different roles quickly.

    Learn the map: Respawn locations help players familiarize themselves with key areas.

    Adds a unique phase: The stabilization period creates a distinct early-game dynamic with the time-pressure from damage.

    Fully customizable: Tweak it to fit your server's needs.

Hope this plugin makes those early game deaths a bit less punishing! Let me know what you think or if you run into any issues. Download link/instructions below!

Key Translation Choices & Rationale:

    Title & Intro: Made it sound like a community announcement ("Hey folks!", "Plugin Release"). Clearly stated the main purpose upfront ("give new players a fighting chance").

    "现实稳定器" -> "Reality Stabilizer": Direct translation fits the SCP theme perfectly.

    "前期弱势群体" -> "players who die early" / "new or unlucky players": More natural community phrasing than "disadvantaged groups". Focuses on the gameplay effect.

    "玩到角色" -> "experience different roles" / "try different roles": Captures the intent of playing various classes.

    Structure: Kept the logical flow (Core Concept -> Key Features -> Game Flow -> Tech) but presented it as a forum post with clear headings and bullet points.

    Terminology:

        "轻收容区"/"重收容区" -> "Light Containment Zone"/"Heavy Containment Zone": Standard SCP:SL terms.

        "Class D", "Scientist", "Facility Guard": Kept consistent.

        "全服通知" -> "Server-wide notification/message/alert": Clear meaning.

        "复活" -> "Respawn": Standard gaming term.

        "倒计时" -> "Countdown".

        "时空扰动" -> "Temporal Disturbance": Sounds sci-fi and fitting.

        "防刷屏机制" -> "Anti-spam cooldown": Common term in modding/gaming.

        "高度可配置" -> "Highly Configurable": Standard term.

        "EXILED框架" -> "EXILED framework": Proper name retention.

        "协程" -> "Coroutines": Technical term understood by developers.

        "响应式UI提示" -> "Reactive UI Hints": Describes the behavior (color/size changes).

    Community Tone: Used phrases like "Hey folks!", "Why You'll Like It", "Hope this plugin...", "Let me know what you think". Added brief explanations of benefits ("Reduces early-round frustration").

    Game Flow: Used arrows (->) replaced with a numbered list describing the sequence more naturally for a reader.

    Call to Action: Ended with a typical community sign-off ("Hope this...", "Let me know...", implied download link).

    Emphasis: Used bold for key features and terms to improve readability in a forum post format.
