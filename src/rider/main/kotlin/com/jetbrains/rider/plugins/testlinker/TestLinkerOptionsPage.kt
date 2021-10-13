package com.jetbrains.rider.plugins.testlinker

import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class TestLinkerOptionsPage : SimpleOptionsPage("TestLinker Options", "TestLinkerOptionsPage") {
    override fun getId(): String {
        return "TestLinkerOptionsPage"
    }
}