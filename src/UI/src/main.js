import Vue from 'vue'
import App from './App'
import router from './router'
import store from './store'

import ElementUI from 'element-ui';
import 'element-ui/lib/theme-chalk/index.css';
import './assets/css/web.css';
import animate from 'animate.css'
import VueResource from 'vue-resource' 
import 'font-awesome/css/font-awesome.min.css' 
import VueParticles from 'vue-particles'  

import VueI18n from 'vue-i18n'
import enLocale from 'element-ui/lib/locale/lang/en'
import zhLocale from 'element-ui/lib/locale/lang/zh-CN'


 
import { basic } from '@/common/basic.js'
Vue.prototype.basic = basic 

// Vue.config.productionTip = false;
// Vue.config.silent = true

Vue.use(VueI18n)  
Vue.use(animate)
Vue.use(VueResource)
Vue.use(VueParticles)  
 
const i18n = new VueI18n({
  locale: basic.isEmpty(localStorage.getItem("locale")) ? 'en-us': localStorage.getItem("locale"),  
  messages: {
    'en-us': Object.assign(require("../static/lang/en-us.json"), enLocale),
    'zh-cn': Object.assign(require("../static/lang/zh-cn.json"), zhLocale), 
  }

}) 

Vue.use(ElementUI, {
  i18n: (key, value) => i18n.t(key, value)
})


Vue.config.productionTip = false 

var vue = new Vue({
  el: '#app',
  store, 
  i18n,
  router, 
  render: h => h(App),
  created:function () {    

    var configUrl = window.location.href.substring(0, window.location.href.indexOf('index.html')) + "config.json";   

    setHttpFilter();  

    loadState();   

    loadLanguage(); 

  },
  methods:{
  
  } 

})

 function loadState() {

  var token = store.state.token;  

  if (basic.isEmpty(token)) {

    token = localStorage.getItem("token");  

    if (basic.isEmpty(token)) {   

        router.push({ path: '/login' })  
    }
    else {
      store.commit('set_token',token);
    }
  }

} 


 function loadLanguage() {    
 
  Vue.http.get("GetLanguage").then(response => {
 
    var lang = response.body.data;  

    localStorage.setItem('locale',lang);

    Vue.http.get(`/static/lang/${lang}.json`).then(res =>{   
      
      store.commit('set_lang',res.body); 

    }); 

  }); 
  
} 


function setHttpFilter() {   

  Vue.http.options.root = "http://localhost:5010/HttpReportsData";  

  // VueResource 请求拦截器
  Vue.http.interceptors.push((request, next) => {

    var token = store.state.token; 

    if (!basic.isEmpty(token)) {
      
      request.headers.set('HttpReports.Auth.Token',token); 

    }  

    next(function (response) { 

      if (response.status != 200) {

        if (response.status == 401) { 

          router.push({ path: '/login' })  
          return;
        } 

        this.$message.error('error');
        return;

      }

      return response;

    });

    
  })

}

router.beforeEach((to, from, next) => { 
 
  if(to.path != '/login'){
    loadState(); 
  }     

  vue.$store.commit("set_route",to.path);   

  next();

});


