Ext.define('RuleEditor.store.RuleStore', {
    extend: 'Ext.data.Store',

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || { };
        me.callParent([Ext.apply({
            storeId: 'RuleStore',
            proxy: {
                type: 'ajax',
                url: 'data/rules.json',
                reader: {
                    type: 'json',
                    root: 'results'
                }
            },
            fields: [
                { name: 'id' },
                { name: 'name' },
                { name: 'ruleset' }
            ]
        }, cfg)]);
    }
});
