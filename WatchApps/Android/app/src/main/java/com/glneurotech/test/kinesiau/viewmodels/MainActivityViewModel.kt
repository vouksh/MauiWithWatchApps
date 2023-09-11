package com.example.mauiwithwatch.viewmodels

import android.app.Application
import androidx.lifecycle.AndroidViewModel

class MainActivityViewModel(application: Application) :
    AndroidViewModel(application) {
    var connectedDeviceName: String = "Not Connected"
}