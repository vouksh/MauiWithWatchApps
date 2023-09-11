/* While this template provides a good starting point for using Wear Compose, you can always
 * take a look at https://github.com/android/wear-os-samples/tree/main/ComposeStarter and
 * https://github.com/android/wear-os-samples/tree/main/ComposeAdvanced to find the most up to date
 * changes to the libraries and their usages.
 */

package com.example.mauiwithwatch.presentation

import android.os.Bundle
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Devices
import androidx.compose.ui.tooling.preview.Preview
import androidx.lifecycle.lifecycleScope
import androidx.wear.ambient.AmbientLifecycleObserver
import androidx.wear.compose.material.Button
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.Text
import com.example.mauiwithwatch.R
import com.example.mauiwithwatch.presentation.theme.MauiWatchTheme
import com.example.mauiwithwatch.services.AppStatus
import com.example.mauiwithwatch.services.Constants
import com.example.mauiwithwatch.services.PhoneInteraction
import com.example.mauiwithwatch.viewmodels.MainActivityViewModel
import com.google.android.gms.wearable.CapabilityClient
import com.google.android.gms.wearable.CapabilityInfo
import com.google.android.gms.wearable.Wearable
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.tasks.await
import kotlinx.coroutines.withContext

class MainActivity :
    ComponentActivity(),
    AmbientLifecycleObserver.AmbientLifecycleCallback,
    CapabilityClient.OnCapabilityChangedListener {
    private lateinit var ambientController: AmbientLifecycleObserver
    private val viewModel by viewModels<MainActivityViewModel>()
    private val capabilityClient by lazy { Wearable.getCapabilityClient(this) }
    private val dataClient by lazy { Wearable.getDataClient(this) }
    private val messageClient by lazy { Wearable.getMessageClient(this) }
    private val phoneInteraction by lazy { PhoneInteraction() }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        try {
            ambientController = AmbientLifecycleObserver(this, this)
            phoneInteraction.sendAppStatus(AppStatus.Running)
        } catch (e: Exception) {
            Log.e(Constants.LOG_UI, "Error creating app", e)
        }
        setContent {
            WearApp(viewModel.connectedDeviceName, ::buttonClicked)
        }
    }
    fun checkForCompanionApp() {
        try {
            lifecycleScope.launch {
                val capabilityInfo = capabilityClient.getCapability(
                    Constants.CAPABILITY_KINESIAU,
                    CapabilityClient.FILTER_ALL
                ).await()
                withContext(Dispatchers.Main) {

                    val firstNode = capabilityInfo.nodes.firstOrNull()
                    if (firstNode != null) {
                        phoneInteraction.primaryNode = firstNode
                        viewModel.connectedDeviceName = firstNode.displayName
                        Log.i(
                            Constants.LOG_PHONE,
                            "Found phone with companion app ${firstNode.displayName} (${firstNode.id})"
                        )
                    } else {
                        Log.e(Constants.LOG_PHONE, "No nodes found")
                    }
                }
            }
        }
        catch (e: Exception) {
            Log.e(Constants.LOG_PHONE, "Error checking for companion apps", e)
        }
    }
    private fun buttonClicked() {
        checkForCompanionApp()
        phoneInteraction.setNode()
    }

    override fun onResume() {
        super.onResume()
        dataClient.addListener(phoneInteraction)
        messageClient.addListener(phoneInteraction)
        capabilityClient.addListener(this, Constants.CAPABILITY_KINESIAU)
        checkForCompanionApp()
        //phoneInteraction.connect()
    }

    override fun onStop() {
        super.onStop()
        //phoneInteraction.disconnect()
    }

    override fun onPause() {
        super.onPause()
        dataClient.removeListener(phoneInteraction)
        messageClient.removeListener(phoneInteraction)
        capabilityClient.removeListener(this, Constants.CAPABILITY_KINESIAU)
    }
    override fun onCapabilityChanged(p0: CapabilityInfo) {
        Log.i(Constants.LOG_PHONE, "onCapabilityChanged")
    }
    companion object {
    }
}

@Composable
fun WearApp(
    greetingName: String,
    buttonClicked: () -> Unit) {
    MauiWatchTheme {
        /* If you have enough items in your list, use [ScalingLazyColumn] which is an optimized
         * version of LazyColumn for wear devices with some added features. For more information,
         * see d.android.com/wear/compose.
         */
        Column(
                modifier = Modifier
                    .fillMaxSize()
                    .background(MaterialTheme.colors.background),
                verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Greeting(greetingName = greetingName)
            Button(onClick = buttonClicked) {
                Text(
                    text = "Check",
                    modifier = Modifier.fillMaxWidth(.75f),
                    textAlign = TextAlign.Center)
            }
        }
    }
}

@Composable
fun Greeting(greetingName: String) {
    Text(
            modifier = Modifier.fillMaxWidth(),
            textAlign = TextAlign.Center,
            color = MaterialTheme.colors.primary,
            text = stringResource(R.string.hello_world, greetingName)
    )
}

@Preview(device = Devices.WEAR_OS_SMALL_ROUND, showSystemUi = true)
@Composable
fun DefaultPreview() {
    WearApp("Preview Android", {})
}