# Padel Point - Padel Tennis Ranking Matches App

## Introduction

Padel Point is a mobile/web-app application designed to facilitate ranking matches for Padel Tennis enthusiasts. The app provides a platform for players to compete in matches, and based on the match results, it calculates and updates the players' Elo ratings. The Elo rating system is used to assess the skill level of players based on their match outcomes and the relative skill levels of their opponents.

## Elo Rating System Explained

At our platform, we use a variation of the Elo rating system, originally developed for chess, to rank padel players. The system provides a fair and effective way to determine a player's skill level relative to others, taking into account both individual and team performance in matches. Each player starts with an initial score, which changes based on their performance in matches as well as a skill calibration questionnaire.

### Initial Elo Rating

When a player joins our platform, their Elo rating doesn't start from a baseline score. Instead, we aim to provide a more accurate starting point by conducting an Initial Player Skill Calibration Quiz. This allows us to take into account the player's previous experience and current skill level in padel as well as other racket sports.

The quiz evaluates factors such as whether the player has played other racket sports before, their skill level in these sports, years of experience, and the self-assessed padel skill level. Depending on the responses, the player's initial Elo rating is calculated.

This calibrated starting point aims to provide a better matchmaking experience from the start. For instance, a beginner player will not have to face highly experienced players in their early matches, and an experienced player can immediately face challenging opponents suitable for their skill level.

As with other aspects of our rating system, the exact formula for the initial Elo calculation is set internally and is not disclosed in the public documentation.

### How Does the Elo System Work?

The Elo rating changes after every match. The system begins by predicting the outcome of the match, referred to as the 'expected score', based on the current ratings of the players. The actual match outcome is then compared to this expected score to calculate the new ratings. The extent of rating change depends on a player's actual performance against the expected performance and the difference in ratings between players. Players gain more points for beating higher-rated opponents and lose more points for losing to lower-rated opponents.

Formula: Expected Score (E) = 1 / (1 + 10^(opponent's rating - player's rating) / 400)

### K-Factor

The K-Factor is a value used in the Elo rating formula to determine how drastically a player's rating will change after a match. The value of K is dynamic and depends on a player's current Elo rating. Players with a lower Elo rating have a higher K-factor, meaning their ratings are more volatile and can change rapidly. Conversely, for players with higher Elo ratings, the K-factor is lower, which means their ratings are more stable and change at a slower rate.

By doing so, we allow beginners to climb the ranks faster based on their performance and provide experienced players with a stable rating that's less affected by single match results.

Please note that the actual values and Elo ranges for K-factor adjustments are set internally by our team and are not disclosed in the public documentation.

### Importance of Match Score

In addition to the win or loss, the final score of the match influences the adjustment of Elo ratings. A larger margin of victory or defeat will result in a greater change in the player's Elo rating. A closely fought match, on the other hand, results in a smaller change. So, even if a player loses, a good fight can help maintain their rating.

Formula: New Rating = Old Rating + K * (Actual Score - Expected Score)

### Team Dynamics

In team matches, the change in a player's Elo rating considers the average rating of the opposing team. The adjustment also accounts for the individual player's current rating and the match outcome. This mechanism ensures fairer rating changes, allowing lower-ranked players to benefit from victories against high-ranked opponents and mitigating their rating loss in defeats.

### Elo History and Notifications

Each player's Elo history is preserved on the platform, detailing the changes in their Elo rating over time, the dates of changes, the reasons, and the matches causing the changes. This allows players to track their progress, identify patterns, and strategize for future matches. Additionally, players receive notifications every time their Elo changes to keep them up to date.

## Conclusion

The Elo rating system on our platform provides a fair, transparent, and dynamic method to rank padel players. While it may seem intricate at first, its core principle is straightforward - performance is rewarded. Keep playing, aim for victories against stronger opponents, and watch your rating soar!
