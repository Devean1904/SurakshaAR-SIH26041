package com.surakshaar.data.repository

import android.content.Context
import android.content.SharedPreferences
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import com.surakshaar.data.model.SiteModel

object SiteStorage {

    private const val PREFS_NAME = "surakshaar_sites"
    private const val KEY_SITES = "sites_json"

    private lateinit var prefs: SharedPreferences
    private val gson = Gson()

    fun init(context: Context) {
        prefs = context.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE)
    }

    fun saveSite(site: SiteModel) {
        val sites = getAllSites().toMutableList()
        sites.removeAll { it.siteId == site.siteId }
        sites.add(site)
        prefs.edit().putString(KEY_SITES, gson.toJson(sites)).apply()
    }

    fun getAllSites(): List<SiteModel> {
        val json = prefs.getString(KEY_SITES, null) ?: return emptyList()
        return try {
            val type = object : TypeToken<List<SiteModel>>() {}.type
            gson.fromJson(json, type)
        } catch (e: Exception) {
            emptyList()
        }
    }

    fun getSite(siteId: String): SiteModel? {
        return getAllSites().find { it.siteId == siteId }
    }

    fun deleteSite(siteId: String) {
        val sites = getAllSites().toMutableList()
        sites.removeAll { it.siteId == siteId }
        prefs.edit().putString(KEY_SITES, gson.toJson(sites)).apply()
    }

    fun getActiveSites(): List<SiteModel> {
        return getAllSites().filter { it.isActive }
    }
}
