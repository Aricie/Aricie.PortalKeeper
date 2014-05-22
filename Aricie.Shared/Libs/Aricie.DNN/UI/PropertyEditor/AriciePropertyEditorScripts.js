﻿/// <reference name="MicrosoftAjax.js"/>

Type.registerNamespace('Aricie.DNN.AriciePropertyEditorScripts');
var AriciePropertyEditorScripts;

Aricie.DNN.AriciePropertyEditorScripts = function (element) {
    Aricie.DNN.AriciePropertyEditorScripts.initializeBase(this, [element]);

    AriciePropertyEditorScripts = this;
    this._clientId = null;
    this._hash = null;
};
Aricie.DNN.AriciePropertyEditorScripts.prototype = {

    initialize: function () {
        Aricie.DNN.AriciePropertyEditorScripts.callBaseMethod(this, 'initialize');
        initialisePropertyEditorsScripts();
    },

    dispose: function () {
        Aricie.DNN.AriciePropertyEditorScripts.callBaseMethod(this, 'dispose');
    },

    // ClientId
    get_clientId: function () {
        return this._clientId;
    },

    set_clientId: function (value) {
        if (this._clientId !== value) {
            this._clientId = value;
            this.raisePropertyChanged('clientId');
        }
    },

    // Hash
    get_hash: function () {
        return this._hash;
    },

    set_hash: function (value) {
        if (this._hash !== value) {
            this._hash = value;
            this.raisePropertyChanged('hash');
        }
    }
};

function initialisePropertyEditorsScripts() {

    //resetUnusedCookies();
    initJSON();
    var selector;
    var cookieAccName;
    var cookieTabName;

    // accordion
    selector = ".aricie_pe_accordion" + "-" + AriciePropertyEditorScripts.get_clientId();
    cookieAccName = 'cookieAccordion' + AriciePropertyEditorScripts.get_clientId() + AriciePropertyEditorScripts.get_hash();

    // on fait l'évaluation du noeud un minimum
    var selectedNodes = jQuery(selector);
    if (selectedNodes.length > 0) {
        jQuery.each(selectedNodes, function (i, selectedNodeDom) {
            var selectedNode = jQuery(selectedNodeDom);
            var cookieVal = getAdvanceVariableValue(cookieAccName); //eval(jQuery.cookie(cookieAccName));
            if (cookieVal == undefined || cookieVal === -1) { // si on a pas de cookie décrivant l'état de cet accordéon, il est fermé
                cookieVal = false;
            } else {
                cookieVal = parseInt(cookieVal);
            }
            selectedNode.accordion(
            {
                active: cookieVal,
                heightStyle: "content",
                clearStyle: true,
                autoHeight: false,
                collapsible: true
            });

            jQuery('> h3.ui-accordion-header>a', selectedNode).click(function () {
                var h3 = jQuery(this).parent();
                var index = h3.parent().children('h3.ui-accordion-header').index(h3);
                var cookieVal = getAdvanceVariableValue(cookieAccName); //eval(jQuery.cookie(cookieAccName));

                if (cookieVal === index) {

                    setAdvanceVariableValue(cookieAccName, null);
                } else {

                    setAdvanceVariableValue(cookieAccName, index);
                }
            });

            jQuery.each(jQuery(" > h3", selectedNode), function (i, h3) {
                jQuery.each(jQuery(h3).data("events") || jQuery._data(h3, "events"), function (j, event) {
                    jQuery.each(event, function (k, h) {
                        if (h.type === 'click') {
                            jQuery(h3).unbind(h.type, h.handler);
                            jQuery(h3).children("a").click(function () {
                                jQuery(this).parent().bind(h.type, h.handler);
                                jQuery(this).parent().triggerHandler(h.type);
                                jQuery(this).parent().unbind(h.type, h.handler);
                            });
                        }
                    });
                });
            });

        });
    }

    // tabs
    selector = ".aricie_pe_tabs" + "-" + AriciePropertyEditorScripts.get_clientId();
    cookieTabName = 'cookieTab' + AriciePropertyEditorScripts.get_clientId() + AriciePropertyEditorScripts.get_hash();
    // var lis = jQuery(selector).find('ul').eq(0).find('li');
    jQuery(selector).tabs({
        /*cookie: { name: cookieTabName, expires: 1 },*/
        select: function (event, ui) {
            //jQuery.cookie(cookieTabName, lis.index(ui.newTab));
            setAdvanceVariableValue(cookieTabName, ui.index); //lis.index(ui.newTab) -->newTab=undefined
            var resultat = performASPNetValidation();
            return resultat;
        },
        activate: function (e, ui) {
            //jQuery.cookie(cookieTabName, lis.index(ui.newTab));
            var currentIndex = ui.newTab.parent().find("li").index(ui.newTab);
            setAdvanceVariableValue(cookieTabName, currentIndex); //lis.index(ui.newTab) -->newTab=undefined
            var resultat = performASPNetValidation();
            return resultat;
        },
        active: (parseInt(getAdvanceVariableValue(cookieTabName)) || 0),
        selected: (parseInt(getAdvanceVariableValue(cookieTabName)) || 0)
    });
    /*if (parseFloat(jQuery.ui.version) > 1.8) {
        jQuery(selector).tabs("option", "active", parseInt(getAdvanceVariableValue(cookieTabName)) || 0);
    } else {
        jQuery(selector).tabs("option", "selected", parseInt(getAdvanceVariableValue(cookieTabName)) || 0);
    }*/
}
function initJSON() {
    window.JSON = JSON || {};

    // implement JSON.stringify serialization
    JSON.stringify = JSON.stringify || function (obj) {

        var t = typeof (obj);
        if (t !== "object" || obj === null) {

            // simple data type
            if (t === "string") { obj = '"' + obj + '"'; }
            return String(obj);

        } else {

            // recurse array or object
            var n, v, json = [], arr = (obj && obj.constructor == Array);

            for (n in obj) {
                v = obj[n]; t = typeof (v);

                if (t === "string") {
                    v = '"' + v + '"';
                } else {
                    if (t == "object" && v !== null) {
                        v = JSON.stringify(v);
                    }
                }

                json.push((arr ? "" : '"' + n + '":') + String(v));
            }

            return (arr ? "[" : "{") + String(json) + (arr ? "]" : "}");
        }
    };

    // implement JSON.parse de-serialization
    JSON.parse = JSON.parse || function (str) {
        if (str === "") {
            str = '""';
        }
        eval("var p=" + str + ";");
        return p;
    };
}
function getAdvanceVariableValue(key) {
    var toReturn = "";
    var Adv = dnn.getVar("AdvVar");
    var AdvObj;
    if (Adv != undefined) {

        if (JSON) {
            AdvObj = JSON.parse(Adv);
        } else {
            AdvObj = eval(Adv);
        }
        if (AdvObj != undefined) {
            toReturn = AdvObj[key];
        }
    }
    return toReturn;
}
function setAdvanceVariableValue(key, value) {
    var Adv = dnn.getVar("AdvVar");
    var AdvObj;
    if (Adv != undefined) {
        if (JSON) {
            AdvObj = JSON.parse(Adv);
        } else {
            AdvObj = eval(Adv);
        }
    } else {
        AdvObj = {};
    }
    AdvObj[key] = value;
    Adv = JSON.stringify(AdvObj);
    dnn.setVar("AdvVar", Adv);
}
function performASPNetValidation() {
    if (typeof Page_ClientValidate === "function") {
        var validated = Page_ClientValidate();
        return validated;
    }
    return true;
}

/*function resetUnusedCookies() {
    var cookies = get_cookies_array();

    for (var cookieName in cookies) {
        if (cookieName.toString().indexOf('cookieAccordion') === 0 || cookieName.toString().indexOf('cookieTab') === 0) {
            var hash = cookieName.replace('cookieAccordion', '').replace('cookieTab', '');

            if (jQuery('[hash="' + hash + '"]').length == 0) {
                jQuery.removeCookie(cookieName);
            }
        }
    }
}

function get_cookies_array() {

    var cookies = {};

    if (document.cookie && document.cookie != '') {
        var split = document.cookie.split(';');
        for (var i = 0; i < split.length; i++) {
            var name_value = split[i].split("=");
            name_value[0] = name_value[0].replace(/^ /, '');
            cookies[decodeURIComponent(name_value[0])] = decodeURIComponent(name_value[1]);
        }
    }

    return cookies;

}*/

function SelectAndActivateParentHeader(src) {
    var targetItem = jQuery(src).parent().parent().find(">a");
    targetItem.attr('onclick', '');
    targetItem.click();
    //return false;
    window.location.hash = targetItem.attr("href");
}

// Register the class as a type that inherits from Sys.UI.Control.
Aricie.DNN.AriciePropertyEditorScripts.registerClass('Aricie.DNN.AriciePropertyEditorScripts', Sys.UI.Control);

if (typeof (Sys) !== 'undefined') { Sys.Application.notifyScriptLoaded(); }