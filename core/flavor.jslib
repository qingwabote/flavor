mergeInto(LibraryManager.library, {
    flavor_minigame: function () {
        return typeof wx !== "undefined";
    },
});
