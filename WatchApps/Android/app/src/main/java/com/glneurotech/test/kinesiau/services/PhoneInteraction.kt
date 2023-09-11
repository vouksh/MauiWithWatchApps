package com.example.mauiwithwatch.services

import android.util.Log
import com.google.android.gms.wearable.CapabilityInfo
import com.google.android.gms.wearable.DataClient
import com.google.android.gms.wearable.DataEventBuffer
import com.google.android.gms.wearable.DataMapItem
import com.google.android.gms.wearable.MessageClient
import com.google.android.gms.wearable.MessageEvent
import com.google.android.gms.wearable.Node
import com.google.android.gms.wearable.PutDataMapRequest
import com.google.android.gms.wearable.Wearable
import com.google.android.gms.wearable.WearableListenerService
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.launch
import kotlinx.coroutines.tasks.await
import kotlinx.datetime.Clock
import kotlinx.datetime.LocalDateTime
import kotlinx.datetime.TimeZone
import kotlinx.datetime.toLocalDateTime
import kotlinx.serialization.Serializable
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import java.util.Timer
import java.util.TimerTask

class PhoneInteraction : WearableListenerService(),
    DataClient.OnDataChangedListener,
    MessageClient.OnMessageReceivedListener {
    private var statusTimer: Timer = Timer("statusTimer", true)
    private val messageClient by lazy { Wearable.getMessageClient(applicationContext) }
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Main.immediate)
    private var nodeId: String = ""
    var primaryNode: Node? = null

    init {
        statusTimer.scheduleAtFixedRate(
            object : TimerTask() {
                override fun run() {
                    try {
                        sendAppStatus(AppStatus.Running)
                    } catch (e: Exception) {
                        Log.e(Constants.LOG_UI, "Error sending app status", e)
                    }
                }
            },
            0,
            30 * 1000
        )
    }
    fun setNode() {
        scope.launch {
            try {
                val nodes = Wearable.getNodeClient(applicationContext).connectedNodes.await()
                for (node in nodes) {
                    if (node.isNearby) {
                        primaryNode = node
                        nodeId = node.id
                        Log.i(Constants.LOG_PHONE, "Found paired device ${node.displayName} ($nodeId)")
                        break;
                    }
                }
            } catch (cancellationException: CancellationException) {
                throw cancellationException
            } catch (exception: Exception) {
                Log.d(Constants.LOG_PHONE, "Querying nodes failed: $exception")
            }
        }
    }
    fun sendResponse(stringData: String) {
        if (primaryNode == null) {
            setNode()
            return
        }
        val now = Clock.System.now().toLocalDateTime(TimeZone.currentSystemDefault());
        var respCmd = CommandObject(Constants.Watch.WATCH_STATUS, stringData, 0.0, now)
        var jsonCmd = Json.encodeToString(respCmd);
        Wearable.getMessageClient(applicationContext).sendMessage(primaryNode?.id.toString(), Constants.MESSAGE_DATA, jsonCmd.toByteArray(Charsets.UTF_8))
    }

    fun sendAppStatus(status: AppStatus) {
        if (primaryNode == null) {
            //setNode()
            return
        }
        try {
            Log.i(Constants.LOG_PHONE, "Sending app status to device ${status.name}")
            val now = Clock.System.now().toLocalDateTime(TimeZone.currentSystemDefault());
            val respCmd = CommandObject(Constants.Watch.WATCH_STATUS, "", status.ordinal.toDouble(), now)
            val jsonCmd = Json.encodeToString(respCmd);
            scope.launch {
                Wearable.getMessageClient(applicationContext).sendMessage(
                    primaryNode?.id.toString(),
                    Constants.MESSAGE_DATA, jsonCmd.toByteArray(Charsets.UTF_8)
                ).await()
                val request = PutDataMapRequest.create(Constants.FILE_TRANSFER).apply {
                    dataMap.putString("COMMAND", jsonCmd)
                }.asPutDataRequest()
                    .setUrgent()
                Wearable.getDataClient(applicationContext).putDataItem(request).await()
            }

        }catch (e: Exception) {
            Log.e(Constants.LOG_PHONE, "Error sending app status", e)
        }
    }
    override fun onDataChanged(dataEvents: DataEventBuffer) {
        dataEvents.forEach {dataEvent ->
            try {
                val dataItem = DataMapItem.fromDataItem(dataEvent.dataItem)
                val cmdBytes = dataItem.dataMap.getString("COMMAND")
                if (!cmdBytes.isNullOrEmpty()) {
                    val cmdObj = Json.decodeFromString<CommandObject>(cmdBytes)
                    Log.i(
                        Constants.LOG_PHONE,
                        "Received command: ${cmdObj.command} (${dataItem.uri.host})"
                    )
                }
            }
            catch (e: Exception) {
                Log.e(Constants.LOG_PHONE, "Data item is not a valid command", e)
            }
        }
        super.onDataChanged(dataEvents)
    }

    override fun onMessageReceived(messageEvent: MessageEvent) {
        Log.i(Constants.LOG_PHONE, "onMessageReceived")
        super.onMessageReceived(messageEvent)
    }

    override fun onCapabilityChanged(p0: CapabilityInfo) {
        Log.i(Constants.LOG_PHONE, "onCapabilityChanged")
        super.onCapabilityChanged(p0)
    }
}

@Serializable
data class CommandObject(val command: String, val stringData: String, val numberData: Double, val timeStamp: LocalDateTime)

class Constants {
    class Phone {
        companion object {
            const val QUERY_STATUS: String = "QUERY_STATUS"
            const val TRANSFER_FILES: String = "TRANSFER_FILES"
        }
    }
    class Watch {
        companion object {
            const val WATCH_STATUS: String = "WATCH_STATUS"
        }
    }
    companion object {
        const val LOG_PHONE = "PhoneInteraction"
        const val LOG_UI = "MainActivity"
        const val MESSAGE_DATA = "/message-data"
        const val FILE_TRANSFER = "/file-transfer"
        const val CAPABILITY_KINESIAU = "kinesiau_phone_app";
    }
}

enum class AppStatus {
    Unknown,
    Running,
    Stopped,
    Error,
    Syncing
}

enum class CommandType {
    ContextUpdate,
    Message,
    FileTransfer,
    UserInfo
}

enum class Phrase {
    Updated,
    Sent,
    Received,
    Replied,
    Transferring,
    Canceled,
    Finished,
    Failed,
}