Ext.Loader.setConfig({ enabled: true });

Ext.application({
    name: 'RuleEditor',
    autoCreateViewport: true,

    stores: ['RuleSetTreeStore', 'RuleStore'],

    launch: function () {

    }
});