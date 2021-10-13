package com.jetbrains.rider.plugins.gototest

import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class GoToTestOptionsPage : SimpleOptionsPage("Go to Test", "GoToTestOptionsPage") {
    override fun getId(): String {
        return "GoToTestOptionsPage"
    }
}