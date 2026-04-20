mergeInto(LibraryManager.library, {
    flavor_minigame: function () {
        return typeof wx !== "undefined";
    },
    flavor_applyUpdate: function () {
        wx.getUpdateManager().applyUpdate();
    },
    flavor_restart:function () {
        wx.restartMiniProgram();
    }
});
