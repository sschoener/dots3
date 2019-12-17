This is a Match 3 game implemented using DOTS and entities in Unity.

Notes:
 * The bulk of the code was written for a larger project and then surgically extracted, so there might be a few loose ends.
 * Note that none of the code is using jobs and all of it was written for Entities 0.2. Only very little attempts were made to make it use features from newer releases.
 * Similarly, the code was written at a time when the Hybrid Renderer did not support per-instance data, which is why this game manually feeds the renderer.
 * Some of the code could be simplified and probably made more sense in the context of the full game. I have cut down some stuff, but if there's something that seems more complicated than necessary, that's probably why.
 * The game currently does not currently enforce that every move creates a match. Not difficult to add, but just not present right now.
 * To get started, check out the `Bootstrap` script. 