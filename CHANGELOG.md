# Changelog

## 1.0.1

Fixes a crash that happened when opening the Steam schema fetcher or scanning local achievements, caused by the AngleSharp dependency not loading the correct version at runtime. The local file scanner also no longer treats unrelated or empty files in the configured folder as achievement data, and shows a clearer message when the game has not created its achievement file yet.

## 1.0.0

Initial open-source release.
